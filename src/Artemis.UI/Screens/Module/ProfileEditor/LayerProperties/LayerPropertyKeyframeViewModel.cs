using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerKeyframeViewModel<T>
    {
        public LayerKeyframeViewModel(LayerPropertyKeyframe<T> keyframe)
        {
            Keyframe = keyframe;
        }

        public LayerPropertyKeyframe<T> Keyframe { get; }
    }
}