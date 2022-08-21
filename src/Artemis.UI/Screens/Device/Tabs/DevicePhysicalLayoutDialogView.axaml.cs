using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public class DevicePhysicalLayoutDialogView : ReactiveUserControl<DevicePhysicalLayoutDialogViewModel>
{
    public DevicePhysicalLayoutDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}