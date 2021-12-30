using Artemis.Core;
using Artemis.UI.Ninject.Factories;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class FolderTreeItemViewModel : TreeItemViewModel
    {
        public FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder, IProfileEditorVmFactory profileEditorVmFactory) : base(parent, folder)
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