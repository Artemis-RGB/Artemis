using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public class DevicePropertiesTabView : ReactiveUserControl<DevicePropertiesTabViewModel>
{
    public DevicePropertiesTabView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ViewModel?.BrowseCustomLayout();
    }
}