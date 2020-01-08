using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree
{
    public class PropertyTreeChildViewModel : PropertyTreeItemViewModel
    {
        public PropertyTreeChildViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
            PropertyInputViewModel = layerPropertyViewModel.GetPropertyInputViewModel();
        }

        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public PropertyInputViewModel PropertyInputViewModel { get; set; }
    }
}