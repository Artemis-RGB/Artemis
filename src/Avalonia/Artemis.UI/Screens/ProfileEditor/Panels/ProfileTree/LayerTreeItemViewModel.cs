using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class LayerTreeItemViewModel : TreeItemViewModel
    {
        public Layer Layer { get; }

        public LayerTreeItemViewModel(Layer layer)
        {
            Layer = layer;
        }
    }
}
