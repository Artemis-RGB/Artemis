using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        private readonly IWindowManager _windowManager;
        private ILayerHintVmFactory _vmFactory;

        public LayerViewModel(ProfileElement layer,
            IRgbService rgbService,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService,
            IWindowManager windowManager,
            ILayerHintVmFactory vmFactory) :
            base(layer, rgbService, profileEditorService, dialogService, profileTreeVmFactory, layerBrushService)
        {
            _windowManager = windowManager;
            _vmFactory = vmFactory;
        }

        public void OpenAdaptionHints()
        {
            _windowManager.ShowDialog(_vmFactory.LayerHintsDialogViewModel(Layer));
        }

        public Layer Layer => ProfileElement as Layer;
        public bool ShowIcons => Layer?.LayerBrush != null;
        public override bool SupportsChildren => false;
    }
}