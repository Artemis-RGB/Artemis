using Artemis.Core.Models.Profile.Abstract;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        public LayerViewModel(TreeItemViewModel parent, ProfileElement layer, IProfileEditorService profileEditorService) : base(parent, layer, profileEditorService)
        {
        }

        public override bool SupportsChildren => false;
    }
}