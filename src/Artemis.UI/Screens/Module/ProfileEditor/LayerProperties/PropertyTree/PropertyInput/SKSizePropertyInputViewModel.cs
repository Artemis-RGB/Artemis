using Artemis.UI.Exceptions;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel
    {
        public SKSizePropertyInputViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            if (layerPropertyViewModel.LayerProperty.Type != typeof(SKSize))
            {
                throw new ArtemisUIException($"This input VM expects a layer property of type {nameof(SKSize)}, " +
                                             $"not the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");
            }
        }
    }
}