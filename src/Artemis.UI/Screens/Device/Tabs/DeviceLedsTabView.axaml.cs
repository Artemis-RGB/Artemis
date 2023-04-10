using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public partial class DeviceLedsTabView : ReactiveUserControl<DeviceLedsTabViewModel>
{
    public DeviceLedsTabView()
    {
        InitializeComponent();
    }

}