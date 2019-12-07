using Artemis.Core.Models.Profile.Abstract;
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
            IFolderViewModelFactory folderViewModelFactory,
            ILayerViewModelFactory layerViewModelFactory) :
            base(parent, folder, profileEditorService, dialogService, folderViewModelFactory, layerViewModelFactory)
        {
        }

        public override bool SupportsChildren => false;
    }
}