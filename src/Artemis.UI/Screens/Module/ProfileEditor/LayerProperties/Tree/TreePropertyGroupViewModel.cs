using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyGroupViewModel
    {
        public TreePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel)layerPropertyBaseViewModel;
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
    }
}