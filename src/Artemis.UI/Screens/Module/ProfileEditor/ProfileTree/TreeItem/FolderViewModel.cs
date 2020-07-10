using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
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
    }
}