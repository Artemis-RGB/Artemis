using Artemis.UI.Exceptions;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class IntPropertyInputViewModel : PropertyInputViewModel
    {
        public IntPropertyInputViewModel(LayerPropertyViewModel layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            if (layerPropertyViewModel.LayerProperty.Type != typeof(int))
            {
                throw new ArtemisUIException("This input VM expects a layer property of type int, " +
                                             $"not the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");
            }
        }
    }
}