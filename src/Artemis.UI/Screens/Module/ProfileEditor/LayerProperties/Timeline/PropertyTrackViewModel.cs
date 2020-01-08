using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackViewModel : PropertyChangedBase
    {
        public PropertyTrackViewModel(PropertyTimelineViewModel propertyTimelineViewModel, LayerPropertyViewModel layerPropertyViewModel)
        {
            PropertyTimelineViewModel = propertyTimelineViewModel;
            LayerPropertyViewModel = layerPropertyViewModel;
            KeyframeViewModels = new BindableCollection<PropertyTrackKeyframeViewModel>();

            PopulateKeyframes();
            UpdateKeyframes(propertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }

        public void PopulateKeyframes()
        {
            foreach (var keyframe in LayerPropertyViewModel.LayerProperty.UntypedKeyframes)
            {
                if (KeyframeViewModels.Any(k => k.Keyframe == keyframe))
                    continue;
                KeyframeViewModels.Add(new PropertyTrackKeyframeViewModel(keyframe));
            }
        }

        public void UpdateKeyframes(int pixelsPerSecond)
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
                keyframeViewModel.Update(pixelsPerSecond);
        }
    }
}