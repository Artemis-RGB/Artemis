using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
        // I hate this about DI, oh well
        public FolderViewModel(ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            ILayerService layerService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory) :
            base(null, folder, profileEditorService, dialogService, layerService, folderVmFactory, layerVmFactory)
        {
        }

        public FolderViewModel(TreeItemViewModel parent,
            ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            ILayerService layerService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory) :
            base(parent, folder, profileEditorService, dialogService, layerService, folderVmFactory, layerVmFactory)
        {
        }

        public override bool SupportsChildren => true;
    }
}