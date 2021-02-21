using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        public LayerViewModel(ProfileElement layer,
            IRgbService rgbService,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService) :
            base(layer, rgbService, profileEditorService, dialogService, profileTreeVmFactory, layerBrushService)
        {
        }

        public Layer Layer => ProfileElement as Layer;
        public bool ShowIcons => Layer?.LayerBrush != null;
        public override bool SupportsChildren => false;
    }
}