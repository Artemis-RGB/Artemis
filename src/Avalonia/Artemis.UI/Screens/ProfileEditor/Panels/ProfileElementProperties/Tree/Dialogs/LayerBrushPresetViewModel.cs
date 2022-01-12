using Artemis.Core.LayerBrushes;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree.Dialogs
{
    public class LayerBrushPresetViewModel : ContentDialogViewModelBase
    {
        public BaseLayerBrush LayerBrush { get; }

        public LayerBrushPresetViewModel(BaseLayerBrush layerBrush)
        {
            LayerBrush = layerBrush;
        }
    }
}
