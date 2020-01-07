using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public abstract class PropertyInputViewModel : PropertyChangedBase
    {
        protected PropertyInputViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
        }

        public LayerPropertyViewModel LayerPropertyViewModel { get; }
    }
}