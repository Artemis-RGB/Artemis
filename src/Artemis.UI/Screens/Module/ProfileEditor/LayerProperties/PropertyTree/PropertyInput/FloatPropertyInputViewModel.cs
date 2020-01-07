using Artemis.UI.Exceptions;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class FloatPropertyInputViewModel : PropertyInputViewModel
    {
        public FloatPropertyInputViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            if (layerPropertyViewModel.LayerProperty.Type != typeof(float))
            {
                throw new ArtemisUIException("This input VM expects a layer property of type float, " +
                                             $"not the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");
            }
        }
    }
}