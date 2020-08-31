using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
        // I hate this about DI, oh well
        public FolderViewModel(ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IRenderElementService renderElementService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory) :
            base(null, folder, profileEditorService, dialogService, renderElementService, folderVmFactory, layerVmFactory)
        {
        }

        public FolderViewModel(TreeItemViewModel parent,
            ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IRenderElementService renderElementService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory) :
            base(parent, folder, profileEditorService, dialogService, renderElementService, folderVmFactory, layerVmFactory)
        {
        }

        public override bool SupportsChildren => true;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var treeItemViewModel in Children)
                    treeItemViewModel.Dispose();
                Children.Clear();
            }

            base.Dispose(disposing);
        }
    }
}