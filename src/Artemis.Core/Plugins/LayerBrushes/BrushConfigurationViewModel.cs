using Stylet;

namespace Artemis.Core.LayerBrushes
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