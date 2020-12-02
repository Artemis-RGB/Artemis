using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Models;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Models;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public sealed class TimelineViewModel : Screen, IDisposable
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
                _profileEditorService.SelectedProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;
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

        private void TimelineOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Core.Timeline.StartSegmentLength) ||
                e.PropertyName == nameof(Core.Timeline.MainSegmentLength) ||
                e.PropertyName == nameof(Core.Timeline.EndSegmentLength))
                Update();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            if (e.PreviousRenderProfileElement != null)
                e.PreviousRenderProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;
            if (e.RenderProfileElement != null)
                e.RenderProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;

            Update();
        }

        #region IDisposable

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            if (_profileEditorService.SelectedProfileElement != null)
                _profileEditorService.SelectedProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;
        }

        #endregion

        #region Command handlers

        public void KeyframeMouseDown(object sender, MouseButtonEventArgs e)
        {
            // if (e.LeftButton == MouseButtonState.Released)
            //     return;

            if (!((sender as Ellipse)?.DataContext is ITimelineKeyframeViewModel viewModel))
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

        public bool CanDuplicateKeyframes => GetAllKeyframeViewModels().Any(k => k.IsSelected);
        public bool CanCopyKeyframes => GetAllKeyframeViewModels().Any(k => k.IsSelected);
        public bool CanDeleteKeyframes => GetAllKeyframeViewModels().Any(k => k.IsSelected);
        public bool CanPasteKeyframes => JsonClipboard.GetData() is KeyframeClipboardModel;

        private TimeSpan? _contextMenuOpenPosition;

        public void ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _contextMenuOpenPosition = GetCursorTime(new Point(e.CursorLeft, e.CursorTop));
        }

        public void ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            _contextMenuOpenPosition = null;
        }

        public void KeyframeContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ITimelineKeyframeViewModel viewModel)
                viewModel.PopulateEasingViewModels();
        }

        public void KeyframeContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.DataContext is ITimelineKeyframeViewModel viewModel)
                viewModel.ClearEasingViewModels();
        }

        public void DeleteKeyframes()
        {
            List<ITimelineKeyframeViewModel> keyframeViewModels = GetAllKeyframeViewModels().Where(k => k.IsSelected).ToList();
            foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels)
                keyframeViewModel.Delete(false);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void DuplicateKeyframes(ITimelineKeyframeViewModel viewModel = null)
        {
            TimeSpan pastePosition = GetPastePosition(viewModel);

            List<ILayerPropertyKeyframe> keyframes = GetAllKeyframeViewModels().Where(k => k.IsSelected).Select(k => k.Keyframe).ToList();
            DuplicateKeyframes(keyframes, pastePosition);
        }

        public void CopyKeyframes()
        {
            List<ILayerPropertyKeyframe> keyframes = GetAllKeyframeViewModels().Where(k => k.IsSelected).Select(k => k.Keyframe).ToList();
            CopyKeyframes(keyframes);
        }

        public void PasteKeyframes(ITimelineKeyframeViewModel viewModel = null)
        {
            TimeSpan pastePosition = GetPastePosition(viewModel);
            PasteKeyframes(pastePosition);
        }

        private TimeSpan GetPastePosition(ITimelineKeyframeViewModel viewModel)
        {
            TimeSpan pastePosition = _profileEditorService.CurrentTime;

            // If a keyframe VM is provided, paste onto there
            if (viewModel != null)
                pastePosition = viewModel.Position;
            // Paste at the position the context menu was opened
            else if (_contextMenuOpenPosition != null)
                pastePosition = _contextMenuOpenPosition.Value;

            return pastePosition;
        }
        
        public List<ILayerPropertyKeyframe> DuplicateKeyframes(List<ILayerPropertyKeyframe> keyframes, TimeSpan pastePosition)
        {
            KeyframeClipboardModel clipboardModel = CoreJson.DeserializeObject<KeyframeClipboardModel>(CoreJson.SerializeObject(new KeyframeClipboardModel(keyframes), true), true);
            return PasteClipboardData(clipboardModel, pastePosition);
        }

        public void CopyKeyframes(List<ILayerPropertyKeyframe> keyframes)
        {
            KeyframeClipboardModel clipboardModel = new KeyframeClipboardModel(keyframes);
            JsonClipboard.SetObject(clipboardModel);
        }

        public List<ILayerPropertyKeyframe> PasteKeyframes(TimeSpan pastePosition)
        {
            KeyframeClipboardModel clipboardObject = JsonClipboard.GetData<KeyframeClipboardModel>();
            return PasteClipboardData(clipboardObject, pastePosition);
        }

        private List<ILayerPropertyKeyframe> PasteClipboardData(KeyframeClipboardModel clipboardModel, TimeSpan pastePosition)
        {
            List<ILayerPropertyKeyframe> pasted = new List<ILayerPropertyKeyframe>();
            if (clipboardModel == null)
                return pasted;
            RenderProfileElement target = _profileEditorService.SelectedProfileElement;
            if (target == null)
                return pasted;

            clipboardModel.Paste(target, pastePosition);
            
            return pasted;
        }

        #endregion

        private TimeSpan GetCursorTime(Point position)
        {
            // Get the parent grid, need that for our position
            double x = Math.Max(0, position.X);
            TimeSpan time = TimeSpan.FromSeconds(x / _profileEditorService.PixelsPerSecond);

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
                time = _profileEditorService.SnapToTimeline(time, TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 5), false, true);

            return time;
        }

        #endregion

        #region Keyframe movement

        public void MoveSelectedKeyframes(TimeSpan cursorTime, ITimelineKeyframeViewModel sourceKeyframeViewModel)
        {
            // Ensure the selection rectangle doesn't show, the view isn't aware of different types of dragging
            SelectionRectangle.Rect = new Rect();

            List<ITimelineKeyframeViewModel> keyframeViewModels = GetAllKeyframeViewModels();
            foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.SaveOffsetToKeyframe(sourceKeyframeViewModel);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                cursorTime = _profileEditorService.SnapToTimeline(
                    cursorTime,
                    TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 5),
                    true,
                    false,
                    keyframeViewModels.Where(k => k != sourceKeyframeViewModel).Select(k => k.Position).ToList()
                );

            sourceKeyframeViewModel.UpdatePosition(cursorTime);

            foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ApplyOffsetToKeyframe(sourceKeyframeViewModel);

            _profileEditorService.UpdateProfilePreview();
        }


        public void ReleaseSelectedKeyframes()
        {
            List<ITimelineKeyframeViewModel> keyframeViewModels = GetAllKeyframeViewModels();
            foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
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
            // Workaround for focus not being applied to the grid causing keybinds not to function
            ((IInputElement) sender).Focus();

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

            Point position = e.GetPosition((IInputElement) sender);
            Rect selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            List<ITimelineKeyframeViewModel> keyframeViewModels = GetAllKeyframeViewModels();
            List<ITimelineKeyframeViewModel> selectedKeyframes = HitTestUtilities.GetHitViewModels<ITimelineKeyframeViewModel>((Visual) sender, SelectionRectangle);
            foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels)
                keyframeViewModel.IsSelected = selectedKeyframes.Contains(keyframeViewModel);

            _mouseDragging = false;
            e.Handled = true;
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition((IInputElement) sender);
                Rect selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;
                e.Handled = true;
            }
        }

        public void SelectKeyframe(ITimelineKeyframeViewModel clicked, bool selectBetween, bool toggle)
        {
            List<ITimelineKeyframeViewModel> keyframeViewModels = GetAllKeyframeViewModels();
            if (selectBetween)
            {
                int selectedIndex = keyframeViewModels.FindIndex(k => k.IsSelected);
                // If nothing is selected, select only the clicked
                if (selectedIndex == -1)
                {
                    clicked.IsSelected = true;
                    return;
                }

                foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;

                int clickedIndex = keyframeViewModels.IndexOf(clicked);
                if (clickedIndex < selectedIndex)
                    foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels.Skip(clickedIndex).Take(selectedIndex - clickedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                else
                    foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels.Skip(selectedIndex).Take(clickedIndex - selectedIndex + 1))
                        keyframeViewModel.IsSelected = true;
            }
            else if (toggle)
            {
                // Toggle only the clicked keyframe, leave others alone
                clicked.IsSelected = !clicked.IsSelected;
            }
            else
            {
                // Only select the clicked keyframe
                foreach (ITimelineKeyframeViewModel keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;
                clicked.IsSelected = true;
            }
        }

        private List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            List<ITimelineKeyframeViewModel> viewModels = new List<ITimelineKeyframeViewModel>();
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in LayerPropertyGroups)
                viewModels.AddRange(layerPropertyGroupViewModel.GetAllKeyframeViewModels(false));

            return viewModels;
        }

        #endregion
    }
}