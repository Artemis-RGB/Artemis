using Artemis.UI.Exceptions;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKPointPropertyInputViewModel : PropertyInputViewModel
    {
        public SKPointPropertyInputViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            if (layerPropertyViewModel.LayerProperty.Type != typeof(SKPoint))
            {
                throw new ArtemisUIException($"This input VM expects a layer property of type {nameof(SKPoint)}, " +
                                             $"not the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");
            }
        }
    }
}