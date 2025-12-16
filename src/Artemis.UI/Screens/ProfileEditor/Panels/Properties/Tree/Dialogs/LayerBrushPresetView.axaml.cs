using Artemis.Core.LayerBrushes;
using Avalonia;
using Avalonia.Input;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.Dialogs;

public partial class LayerBrushPresetView : ReactiveUserControl<LayerBrushPresetViewModel>
{
    public LayerBrushPresetView()
    {
        InitializeComponent();
    }


    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not IDataContextProvider {DataContext: ILayerBrushPreset preset} || ViewModel == null)
            return;

        ViewModel?.SelectPreset(preset);
    }
}