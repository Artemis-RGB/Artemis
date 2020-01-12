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
        private readonly IPropertyTrackViewModelFactory _propertyTrackViewModelFactory;

        public PropertyTimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            IProfileEditorService profileEditorService,
            IPropertyTrackViewModelFactory propertyTrackViewModelFactory)
        {
            _profileEditorService = profileEditorService;
            _propertyTrackViewModelFactory = propertyTrackViewModelFactory;

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

        private void CreateViewModels(LayerPropertyViewModel property)
        {
            PropertyTrackViewModels.Add(_propertyTrackViewModelFactory.Create(this, property));
            foreach (var child in property.Children)
                CreateViewModels(child);
        }

        public void ClearProperties()
        {
            PropertyTrackViewModels.Clear();
        }

        public void UpdateKeyframePositions()
        {
            foreach (var viewModel in PropertyTrackViewModels)
                viewModel.UpdateKeyframes(LayerPropertiesViewModel.PixelsPerSecond);

            UpdateEndTime();
        }

        /// <summary>
        /// Updates the time line's keyframes
        /// </summary>
        public void Update()
        {
            foreach (var viewModel in PropertyTrackViewModels)
                viewModel.PopulateKeyframes();

            UpdateEndTime();
        }
    }
}