using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        public LayerViewModel(TreeItemViewModel parent,
            ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            ILayerService layerService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory) :
            base(parent, folder, profileEditorService, dialogService, layerService, folderVmFactory, layerVmFactory)
        {
        }

        public override bool SupportsChildren => false;
    }
}