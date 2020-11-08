using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public LayerViewModel(ProfileElement layer,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService,
            ISurfaceService surfaceService) :
            base(layer, profileEditorService, dialogService, profileTreeVmFactory, layerBrushService, surfaceService)
        {
            _profileEditorService = profileEditorService;
        }

        public async void CopyElement()
        {
            Layer layer = Layer.CreateCopy();
            
            _profileEditorService.UpdateSelectedProfile();
            // await Task.Delay(200);
            _profileEditorService.ChangeSelectedProfileElement(layer);
        }

        public Layer Layer => ProfileElement as Layer;
        public bool ShowIcons => Layer?.LayerBrush != null;
        public override bool SupportsChildren => false;
    }
}