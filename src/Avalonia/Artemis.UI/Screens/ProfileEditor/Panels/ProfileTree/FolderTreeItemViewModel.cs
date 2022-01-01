using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class FolderTreeItemViewModel : TreeItemViewModel
    {
        public FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder, IProfileEditorVmFactory profileEditorVmFactory, IWindowService windowService) : base(parent, folder, windowService)
        {
            Folder = folder;

            foreach (ProfileElement profileElement in folder.Children)
            {
                if (profileElement is Folder childFolder)
                    Children.Add(profileEditorVmFactory.FolderTreeItemViewModel(this, childFolder));
                else if (profileElement is Layer layer)
                    Children.Add(profileEditorVmFactory.LayerTreeItemViewModel(this, layer));
            }
        }

        public Folder Folder { get; }
    }
}