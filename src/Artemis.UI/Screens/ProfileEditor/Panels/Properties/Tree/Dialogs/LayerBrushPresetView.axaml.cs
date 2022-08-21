using Artemis.Core.LayerBrushes;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.Dialogs;

public class LayerBrushPresetView : ReactiveUserControl<LayerBrushPresetViewModel>
{
    public LayerBrushPresetView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not IDataContextProvider {DataContext: ILayerBrushPreset preset} || ViewModel == null)
            return;

        ViewModel?.SelectPreset(preset);
    }
}