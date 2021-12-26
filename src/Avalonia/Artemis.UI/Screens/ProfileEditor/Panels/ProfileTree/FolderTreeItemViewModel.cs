using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class FolderTreeItemViewModel : TreeItemViewModel
    {
        public Folder Folder { get; }

        public FolderTreeItemViewModel(Folder folder, IProfileEditorVmFactory profileEditorVmFactory)
        {
            Folder = folder;
            Children = new List<TreeItemViewModel>();

            foreach (ProfileElement profileElement in folder.Children)
            {
                if (profileElement is Folder childFolder)
                    Children.Add(profileEditorVmFactory.FolderTreeItemViewModel(childFolder));
                else if (profileElement is Layer layer)
                    Children.Add(profileEditorVmFactory.LayerTreeItemViewModel(layer));
            }
        }

        public List<TreeItemViewModel> Children { get; }
    }
}