using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class LayerTreeItemViewModel : TreeItemViewModel
    {
        public LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer) : base(parent, layer)
        {
            Layer = layer;
        }

        public Layer Layer { get; }
    }
}