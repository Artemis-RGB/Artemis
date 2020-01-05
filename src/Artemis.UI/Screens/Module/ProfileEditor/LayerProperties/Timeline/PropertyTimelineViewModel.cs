using System;
using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTimelineViewModel : PropertyChangedBase
    {
        public PropertyTimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel)
        {
            LayerPropertiesViewModel = layerPropertiesViewModel;
            RailItemViewModels = new BindableCollection<PropertyTrackViewModel>();
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }

        public double Width { get; set; }
        public BindableCollection<PropertyTrackViewModel> RailItemViewModels { get; set; }

        public void UpdateEndTime()
        {
            // End time is the last keyframe + 10 sec
            var lastKeyFrame = RailItemViewModels.SelectMany(r => r.KeyframeViewModels).OrderByDescending(t => t.Keyframe.Position).FirstOrDefault();
            var endTime = lastKeyFrame?.Keyframe.Position.Add(new TimeSpan(0, 0, 0, 10)) ?? TimeSpan.FromSeconds(10);

            Width = endTime.TotalSeconds * LayerPropertiesViewModel.PixelsPerSecond;
        }
    }
}