using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineViewModel : Screen, IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;
        private RectangleGeometry _selectionRectangle;

        public TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertiesViewModel = layerPropertiesViewModel;
            LayerPropertyGroups = layerPropertyGroups;
            SelectionRectangle = new RectangleGeometry();

            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            if (_profileEditorService.SelectedProfileElement != null)
                _profileEditorService.SelectedProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;
            Update();
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }

        public RectangleGeometry SelectionRectangle
        {
            get => _selectionRectangle;
            set => SetAndNotify(ref _selectionRectangle, value);
        }

        public double StartSegmentEndPosition
        {
            get => _startSegmentEndPosition;
            set => SetAndNotify(ref _startSegmentEndPosition, value);
        }

        public double MainSegmentEndPosition
        {
            get => _mainSegmentEndPosition;
            set => SetAndNotify(ref _mainSegmentEndPosition, value);
        }

        public double EndSegmentEndPosition
        {
            get => _endSegmentEndPosition;
            set => SetAndNotify(ref _endSegmentEndPosition, value);
        }

        public double TotalTimelineWidth
        {
            get => _totalTimelineWidth;
            set => SetAndNotify(ref _totalTimelineWidth, value);
        }

        private void Update()
        {
            StartSegmentEndPosition = LayerPropertiesViewModel.StartTimelineSegmentViewModel.SegmentEnd.TotalSeconds * _profileEditorService.PixelsPerSecond;
            MainSegmentEndPosition = LayerPropertiesViewModel.MainTimelineSegmentViewModel.SegmentEnd.TotalSeconds * _profileEditorService.PixelsPerSecond;
            EndSegmentEndPosition = LayerPropertiesViewModel.EndTimelineSegmentViewModel.SegmentEnd.TotalSeconds * _profileEditorService.PixelsPerSecond;

            TotalTimelineWidth = EndSegmentEndPosition;
        }

        private void SelectedProfileElementOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object? sender, RenderProfileElementEventArgs e)
        {
            if (e.PreviousRenderProfileElement != null)
                e.PreviousRenderProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
            if (e.RenderProfileElement != null)
                e.RenderProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;

            Update();
        }

        #region Command handlers

        public void KeyframeMouseDown(object sender, MouseButtonEventArgs e)
        {
            // if (e.LeftButton == MouseButtonState.Released)
            //     return;

            var viewModel = (sender as Ellipse)?.DataContext as ITimelineKeyframeViewModel;
            if (viewModel == null)
                return;

            ((IInputElement) sender).CaptureMouse();
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && !viewModel.IsSelected)
                SelectKeyframe(viewModel, true, false);
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                SelectKeyframe(viewModel, false, true);
            else if (!viewModel.IsSelected)
                SelectKeyframe(viewModel, false, false);

            e.Handled = true;
        }

        public void KeyframeMouseUp(object sender, MouseButtonEventArgs e)
        {
            _profileEditorService.UpdateSelectedProfileElement();
            ReleaseSelectedKeyframes();

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void KeyframeMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.DataContext is ITimelineKeyframeViewModel viewModel)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    MoveSelectedKeyframes(GetCursorTime(e.GetPosition(View)), viewModel);

                e.Handled = true;
            }
        }

        #region Context menu actions

        public void ContextMenuOpening(object sender, EventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.DataContext is ITimelineKeyframeViewModel viewModel)
                viewModel.PopulateEasingViewModels();
        }

        public void ContextMenuClosing(object sender, EventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.DataContext is ITimelineKeyframeViewModel viewModel)
                viewModel.ClearEasingViewModels();
        }

        public void Copy(ITimelineKeyframeViewModel viewModel)
        {
            // viewModel.Copy();
            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.Copy();
        }

        public void Delete(ITimelineKeyframeViewModel viewModel)
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.Delete();
        }

        #endregion

        private TimeSpan GetCursorTime(Point position)
        {
            // Get the parent grid, need that for our position
            var x = Math.Max(0, position.X);
            var time = TimeSpan.FromSeconds(x / _profileEditorService.PixelsPerSecond);

            // Round the time to something that fits the current zoom level
            if (_profileEditorService.PixelsPerSecond < 200)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 5.0) * 5.0);
            else if (_profileEditorService.PixelsPerSecond < 500)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 2.0) * 2.0);
            else
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds));

            // If shift is held, snap to the current time
            // Take a tolerance of 5 pixels (half a keyframe width)
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var tolerance = 1000f / _profileEditorService.PixelsPerSecond * 5;
                if (Math.Abs(_profileEditorService.CurrentTime.TotalMilliseconds - time.TotalMilliseconds) < tolerance)
                    time = _profileEditorService.CurrentTime;
                else if (Math.Abs(_profileEditorService.SelectedProfileElement.StartSegmentLength.TotalMilliseconds - time.TotalMilliseconds) < tolerance)
                    time = _profileEditorService.SelectedProfileElement.StartSegmentLength;
            }

            return time;
        }

        #endregion

        #region Keyframe movement

        public void MoveSelectedKeyframes(TimeSpan cursorTime, ITimelineKeyframeViewModel sourceKeyframeViewModel)
        {
            // Ensure the selection rectangle doesn't show, the view isn't aware of different types of dragging
            SelectionRectangle.Rect = new Rect();

            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.SaveOffsetToKeyframe(sourceKeyframeViewModel);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                cursorTime = _profileEditorService.SnapToTimeline(
                    cursorTime,
                    TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 5),
                    true,
                    false,
                    keyframeViewModels.Where(k => k != sourceKeyframeViewModel).Select(k => k.Position).ToList()
                );
            }

            sourceKeyframeViewModel.UpdatePosition(cursorTime);

            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ApplyOffsetToKeyframe(sourceKeyframeViewModel);

            _profileEditorService.UpdateProfilePreview();
        }


        public void ReleaseSelectedKeyframes()
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ReleaseMovement();
        }

        #endregion

        #region Keyframe selection

        private Point _mouseDragStartPoint;
        private bool _mouseDragging;
        private double _startSegmentEndPosition;
        private double _mainSegmentEndPosition;
        private double _endSegmentEndPosition;
        private double _totalTimelineWidth;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            ((IInputElement) sender).CaptureMouse();

            SelectionRectangle.Rect = new Rect();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
            _mouseDragging = true;
            e.Handled = true;
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (!_mouseDragging)
                return;

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            var keyframeViewModels = GetAllKeyframeViewModels();
            var selectedKeyframes = HitTestUtilities.GetHitViewModels<ITimelineKeyframeViewModel>((Visual) sender, SelectionRectangle);
            foreach (var keyframeViewModel in keyframeViewModels)
                keyframeViewModel.IsSelected = selectedKeyframes.Contains(keyframeViewModel);

            _mouseDragging = false;
            e.Handled = true;
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition((IInputElement) sender);
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;
                e.Handled = true;
            }
        }

        public void SelectKeyframe(ITimelineKeyframeViewModel clicked, bool selectBetween, bool toggle)
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            if (selectBetween)
            {
                var selectedIndex = keyframeViewModels.FindIndex(k => k.IsSelected);
                // If nothing is selected, select only the clicked
                if (selectedIndex == -1)
                {
                    clicked.IsSelected = true;
                    return;
                }

                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;

                var clickedIndex = keyframeViewModels.IndexOf(clicked);
                if (clickedIndex < selectedIndex)
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(clickedIndex).Take(selectedIndex - clickedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
                else
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(selectedIndex).Take(clickedIndex - selectedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
            }
            else if (toggle)
            {
                // Toggle only the clicked keyframe, leave others alone
                clicked.IsSelected = !clicked.IsSelected;
            }
            else
            {
                // Only select the clicked keyframe
                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;
                clicked.IsSelected = true;
            }
        }

        private List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            var viewModels = new List<ITimelineKeyframeViewModel>();
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                viewModels.AddRange(layerPropertyGroupViewModel.GetAllKeyframeViewModels(false));

            return viewModels;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            if (_profileEditorService.SelectedProfileElement != null)
                _profileEditorService.SelectedProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
        }

        #endregion
    }
}