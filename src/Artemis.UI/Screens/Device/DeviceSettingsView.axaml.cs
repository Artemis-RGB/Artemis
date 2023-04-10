using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public partial class DeviceSettingsView : ReactiveUserControl<DeviceSettingsViewModel>
{
    public DeviceSettingsView()
    {
        InitializeComponent();
    }

}