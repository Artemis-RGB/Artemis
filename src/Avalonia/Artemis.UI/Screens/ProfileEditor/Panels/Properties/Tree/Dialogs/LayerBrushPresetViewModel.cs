using Artemis.Core.LayerBrushes;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.Dialogs;

public class LayerBrushPresetViewModel : ContentDialogViewModelBase
{
    public LayerBrushPresetViewModel(BaseLayerBrush layerBrush)
    {
        LayerBrush = layerBrush;
    }

    public BaseLayerBrush LayerBrush { get; }
}