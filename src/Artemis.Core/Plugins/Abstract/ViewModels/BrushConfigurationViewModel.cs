using Artemis.Core.Plugins.LayerBrush.Abstract;
using Stylet;

namespace Artemis.Core.Plugins.Abstract.ViewModels
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