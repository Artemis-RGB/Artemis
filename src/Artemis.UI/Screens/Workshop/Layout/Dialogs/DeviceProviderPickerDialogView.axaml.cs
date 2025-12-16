using Artemis.Core.DeviceProviders;
using Avalonia;
using Avalonia.Input;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.Workshop.Layout.Dialogs;

public partial class DeviceProviderPickerDialogView : ReactiveUserControl<DeviceProviderPickerDialogViewModel>
{
    public DeviceProviderPickerDialogView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not IDataContextProvider {DataContext: DeviceProvider deviceProvider} || ViewModel == null)
            return;
        
        ViewModel?.SelectDeviceProvider(deviceProvider);
    }
}