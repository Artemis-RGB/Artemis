using Artemis.Core.Models.Profile.Abstract;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
        public FolderViewModel(TreeItemViewModel parent, ProfileElement folder, IProfileEditorService profileEditorService) : base(parent, folder, profileEditorService)
        {
        }

        public override bool SupportsChildren => true;
    }
}