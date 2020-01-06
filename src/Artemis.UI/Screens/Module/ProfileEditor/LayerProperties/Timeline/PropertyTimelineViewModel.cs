using System;
using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTimelineViewModel : PropertyChangedBase
    {
        public PropertyTimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel)
        {
            LayerPropertiesViewModel = layerPropertiesViewModel;
            PropertyTrackViewModels = new BindableCollection<PropertyTrackViewModel>();
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
            if (LayerPropertiesViewModel.CurrentTime > endTime)
                LayerPropertiesViewModel.CurrentTime = endTime;
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
            PropertyTrackViewModels.Add(new PropertyTrackViewModel(this, property));
            foreach (var child in property.Children)
                CreateViewModels(child);
        }

        public void ClearProperties()
        {
            PropertyTrackViewModels.Clear();
        }
    }
}