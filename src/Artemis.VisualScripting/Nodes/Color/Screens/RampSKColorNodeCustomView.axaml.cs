using Artemis.UI.Shared.Controls.GradientPicker;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public partial class RampSKColorNodeCustomView : ReactiveUserControl<RampSKColorNodeCustomViewModel>
{
    public RampSKColorNodeCustomView()
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