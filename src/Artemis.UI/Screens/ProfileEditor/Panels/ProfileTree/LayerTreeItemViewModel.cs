using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class LayerTreeItemViewModel : TreeItemViewModel
    {
        public LayerTreeItemViewModel(TreeItemViewModel? parent, 
            Layer layer, 
            IWindowService windowService, 
            IProfileEditorService profileEditorService, 
            IRgbService rgbService,
            ILayerBrushService layerBrushService, 
            IProfileEditorVmFactory profileEditorVmFactory)
            : base(parent, layer, windowService, profileEditorService, rgbService, layerBrushService, profileEditorVmFactory)
        {
            Layer = layer;
        }

        public Layer Layer { get; }

        #region Overrides of TreeItemViewModel

        /// <inheritdoc />
        public override bool SupportsChildren => false;

        #endregion
    }
}