using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.ProfileEditor;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class FolderTreeItemViewModel : TreeItemViewModel
    {
        public FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder, IWindowService windowService, IProfileEditorService profileEditorService,
            IProfileEditorVmFactory profileEditorVmFactory) : base(parent, folder, windowService, profileEditorService, profileEditorVmFactory)
        {
            Folder = folder;
        }

        public Folder Folder { get; }
    }
}