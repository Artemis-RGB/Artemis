using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTimelineViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IPropertyTrackVmFactory _propertyTrackVmFactory;

        public PropertyTimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            IProfileEditorService profileEditorService,
            IPropertyTrackVmFactory propertyTrackVmFactory)
        {
            _profileEditorService = profileEditorService;
            _propertyTrackVmFactory = propertyTrackVmFactory;

            LayerPropertiesViewModel = layerPropertiesViewModel;
            PropertyTrackViewModels = new BindableCollection<PropertyTrackViewModel>();

            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;
            LayerPropertiesViewModel.PixelsPerSecondChanged += OnPixelsPerSecondChanged;

            Execute.PostToUIThread(() => SelectionRectangle = new RectangleGeometry());
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }

        public double Width { get; set; }
        public BindableCollection<PropertyTrackViewModel> PropertyTrackViewModels { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }

        public void Dispose()
        {
            _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;
            LayerPropertiesViewModel.PixelsPerSecondChanged -= OnPixelsPerSecondChanged;
        }

        public void UpdateEndTime()
        {
            // End time is the last keyframe + 10 sec
            var lastKeyFrame = PropertyTrackViewModels.SelectMany(r => r.KeyframeViewModels).OrderByDescending(t => t.Keyframe.Position).FirstOrDefault();
            var endTime = lastKeyFrame?.Keyframe.Position.Add(new TimeSpan(0, 0, 0, 10)) ?? TimeSpan.FromSeconds(10);

            Width = endTime.TotalSeconds * LayerPropertiesViewModel.PixelsPerSecond;

            // Ensure the caret isn't outside the end time
            if (_profileEditorService.CurrentTime > endTime)
                _profileEditorService.CurrentTime = endTime;
        }

        public void PopulateProperties(List<LayerPropertyViewModel> properties)
        {
            var newViewModels = new List<PropertyTrackViewModel>();
            foreach (var property in properties)
                newViewModels.AddRange(CreateViewModels(property));

            PropertyTrackViewModels.Clear();
            PropertyTrackViewModels.AddRange(newViewModels);
            UpdateEndTime();
        }

        public void AddLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            // Determine the index by flattening all the layer's properties
            var index = layerPropertyViewModel.LayerProperty.GetFlattenedIndex();
            if (index > PropertyTrackViewModels.Count)
                index = PropertyTrackViewModels.Count;
            PropertyTrackViewModels.Insert(index, _propertyTrackVmFactory.Create(this, layerPropertyViewModel));
        }

        public void RemoveLayerProperty(LayerPropertyViewModel layerPropertyViewModel)
        {
            var vm = PropertyTrackViewModels.FirstOrDefault(v => v.LayerPropertyViewModel == layerPropertyViewModel);
            if (vm != null)
                PropertyTrackViewModels.Remove(vm);
        }

        public void UpdateKeyframePositions()
        {
            foreach (var viewModel in PropertyTrackViewModels)
                viewModel.UpdateKeyframes(LayerPropertiesViewModel.PixelsPerSecond);

            UpdateEndTime();
        }

        /// <summary>
        ///     Updates the time line's keyframes
        /// </summary>
        public void Update()
        {
            foreach (var viewModel in PropertyTrackViewModels)
                viewModel.PopulateKeyframes();

            UpdateEndTime();
        }

        private void OnSelectedProfileElementUpdated(object sender, ProfileElementEventArgs e)
        {
            Update();
        }

        private void OnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            UpdateKeyframePositions();
        }

        private List<PropertyTrackViewModel> CreateViewModels(LayerPropertyViewModel property)
        {
            var result = new List<PropertyTrackViewModel> {_propertyTrackVmFactory.Create(this, property)};
            foreach (var child in property.Children)
                result.AddRange(CreateViewModels(child));

            return result;
        }

        #region Keyframe movement

        public void MoveSelectedKeyframes(TimeSpan cursorTime)
        {
            // Ensure the selection rectangle doesn't show, the view isn't aware of different types of dragging
            SelectionRectangle.Rect = new Rect();

            var keyframeViewModels = PropertyTrackViewModels.SelectMany(t => t.KeyframeViewModels.OrderBy(k => k.Keyframe.Position)).ToList();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ApplyMovement(cursorTime);

            _profileEditorService.UpdateProfilePreview();
        }


        public void ReleaseSelectedKeyframes()
        {
            var keyframeViewModels = PropertyTrackViewModels.SelectMany(t => t.KeyframeViewModels.OrderBy(k => k.Keyframe.Position)).ToList();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ReleaseMovement();
        }

        #endregion

        #region Keyframe selection

        private Point _mouseDragStartPoint;
        private bool _mouseDragging;

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

            var selectedKeyframes = HitTestUtilities.GetHitViewModels<PropertyTrackKeyframeViewModel>((Visual) sender, SelectionRectangle);
            var keyframeViewModels = PropertyTrackViewModels.SelectMany(t => t.KeyframeViewModels.OrderBy(k => k.Keyframe.Position)).ToList();
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

        public void SelectKeyframe(PropertyTrackKeyframeViewModel clicked, bool selectBetween, bool toggle)
        {
            var keyframeViewModels = PropertyTrackViewModels.SelectMany(t => t.KeyframeViewModels.OrderBy(k => k.Keyframe.Position)).ToList();
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

        #endregion
    }
}