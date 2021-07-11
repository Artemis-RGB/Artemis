using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public class LayerViewModel : TreeItemViewModel
    {
        private readonly IWindowManager _windowManager;
        private readonly ILayerHintVmFactory _vmFactory;

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

        public Layer Layer => ProfileElement as Layer;
        public bool ShowIcons => Layer?.LayerBrush != null;
        public override bool SupportsChildren => false;

        public override bool IsExpanded { get; set; }

        public void OpenAdaptionHints()
        {
            _windowManager.ShowDialog(_vmFactory.LayerHintsDialogViewModel(Layer));
        }

        public override void UpdateBrokenState()
        {
            List<IBreakableModel> brokenModels = ProfileElement.GetBrokenHierarchy().ToList();
            if (!brokenModels.Any())
                BrokenState = null;
            else
            {
                BrokenState = "Layer is in a broken state, click to view exception(s).\r\n" +
                              $"{string.Join("\r\n", brokenModels.Select(e => $" • {e.BrokenDisplayName} - {e.BrokenState}"))}";
            }
        }
    }
}