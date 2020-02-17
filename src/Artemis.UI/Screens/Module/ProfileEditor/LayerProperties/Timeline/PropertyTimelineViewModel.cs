using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTimelineViewModel : PropertyChangedBase
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

            _profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            LayerPropertiesViewModel.PixelsPerSecondChanged += (sender, args) => UpdateKeyframePositions();
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }

        public double Width { get; set; }
        public BindableCollection<PropertyTrackViewModel> PropertyTrackViewModels { get; set; }

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
            PropertyTrackViewModels.Clear();
            foreach (var property in properties)
                CreateViewModels(property);

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

        private void CreateViewModels(LayerPropertyViewModel property)
        {
            PropertyTrackViewModels.Add(_propertyTrackVmFactory.Create(this, property));
            foreach (var child in property.Children)
                CreateViewModels(child);
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

        public void MoveSelectedKeyframes(TimeSpan offset)
        {
            var keyframeViewModels = PropertyTrackViewModels.SelectMany(t => t.KeyframeViewModels.OrderBy(k => k.Keyframe.Position)).ToList();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
            {
                // TODO: Not ideal as this stacks them all if they get to 0, oh well
                if (keyframeViewModel.Keyframe.Position + offset > TimeSpan.Zero)
                {
                    keyframeViewModel.Keyframe.Position += offset;
                    keyframeViewModel.Update(LayerPropertiesViewModel.PixelsPerSecond);
                }
            }

            _profileEditorService.UpdateProfilePreview();
        }
    }
}