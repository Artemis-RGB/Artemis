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
            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }

        public void PopulateKeyframes()
        {
            // Remove old keyframes
            foreach (var viewModel in KeyframeViewModels.ToList())
            {
                if (!LayerPropertyViewModel.LayerProperty.UntypedKeyframes.Contains(viewModel.Keyframe))
                    KeyframeViewModels.Remove(viewModel);
            }

            // Add new keyframes
            foreach (var keyframe in LayerPropertyViewModel.LayerProperty.UntypedKeyframes)
            {
                if (KeyframeViewModels.Any(k => k.Keyframe == keyframe))
                    continue;
                KeyframeViewModels.Add(new PropertyTrackKeyframeViewModel(keyframe));
            }

            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public void UpdateKeyframes(int pixelsPerSecond)
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
                keyframeViewModel.Update(pixelsPerSecond);
        }
    }
}