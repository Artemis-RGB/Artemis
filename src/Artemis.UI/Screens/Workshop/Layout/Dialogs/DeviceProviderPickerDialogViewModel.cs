using System.Collections.ObjectModel;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Workshop.Layout.Dialogs;

public class DeviceProviderPickerDialogViewModel : ContentDialogViewModelBase
{
    public ObservableCollection<DeviceProvider> DeviceProviders { get; }

    public DeviceProviderPickerDialogViewModel(IPluginManagementService pluginManagementService)
    {
        DeviceProviders = new ObservableCollection<DeviceProvider>(pluginManagementService.GetFeaturesOfType<DeviceProvider>());
    }

    public DeviceProvider? DeviceProvider { get; set; }

    public void SelectDeviceProvider(DeviceProvider deviceProvider)
    {
        DeviceProvider = deviceProvider;
        ContentDialog?.Hide();
    }
}