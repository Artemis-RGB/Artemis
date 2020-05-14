using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreeViewModel
    {
        public TreeViewModel(BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups)
        {
            LayerPropertyGroups = layerPropertyGroups;
        }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }
    }
}