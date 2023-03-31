using Artemis.UI.Shared.Controls.GradientPicker;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public partial class ColorGradientNodeCustomView : ReactiveUserControl<ColorGradientNodeCustomViewModel>
{
    public ColorGradientNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void GradientPickerButton_OnFlyoutOpened(GradientPickerButton sender, EventArgs args)
    {
    }

    private void GradientPickerButton_OnFlyoutClosed(GradientPickerButton sender, EventArgs args)
    {
        ViewModel?.StoreGradient();
    }
}