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
            IRgbService rgbService,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService) :
            base(folder, rgbService, profileEditorService, dialogService, profileTreeVmFactory, layerBrushService)
        {
        }

        public override bool SupportsChildren => true;
    }
}