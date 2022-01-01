using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class LayerTreeItemViewModel : TreeItemViewModel
    {
        public LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer, IWindowService windowService) : base(parent, layer, windowService)
        {
            Layer = layer;
        }

        public Layer Layer { get; }
    }
}