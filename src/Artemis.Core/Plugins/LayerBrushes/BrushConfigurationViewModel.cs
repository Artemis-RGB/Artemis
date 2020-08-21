using Artemis.Core.Plugins.LayerBrushes.Internal;
using Stylet;

namespace Artemis.Core.Plugins.LayerBrushes
{
    public abstract class BrushConfigurationViewModel : Screen
    {
        protected BrushConfigurationViewModel(BaseLayerBrush layerBrush)
        {
            LayerBrush = layerBrush;
        }

        public BaseLayerBrush LayerBrush { get; }
    }
}