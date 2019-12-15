using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerElements.Animations
{
    public class AnimationBrushProvider : BrushProvider
    {
        public AnimationBrushProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerElementDescriptor<SlideLayerElement>("Slide animation", "A sliding animation", "ArrowAll");
            AddLayerElementDescriptor<RotationLayerElement>("Rotation animation", "A rotation animation", "CropRotate");
        }

        public override void Dispose()
        {
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }
}