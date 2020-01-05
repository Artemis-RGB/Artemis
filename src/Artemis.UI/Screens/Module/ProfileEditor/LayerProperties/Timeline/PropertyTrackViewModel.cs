using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackViewModel : PropertyChangedBase
    {
        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }

        public PropertyTrackViewModel(PropertyTimelineViewModel propertyTimelineViewModel, LayerPropertyViewModel layerPropertyViewModel)
        {
            PropertyTimelineViewModel = propertyTimelineViewModel;
            LayerPropertyViewModel = layerPropertyViewModel;
            KeyframeViewModels = new BindableCollection<PropertyTrackKeyframeViewModel>();

            PopulateKeyframes();
            UpdateKeyframes(propertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public void PopulateKeyframes()
        {
            foreach (var keyframe in LayerPropertyViewModel.LayerProperty.Keyframes)
            {
                if (KeyframeViewModels.Any(k => k.Keyframe == keyframe))
                    continue;
                KeyframeViewModels.Add(new PropertyTrackKeyframeViewModel(keyframe));
            }
        }

        public void UpdateKeyframes(int pixelsPerSecond)
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
            {
                keyframeViewModel.Update(pixelsPerSecond);
            }
        }
    }
}