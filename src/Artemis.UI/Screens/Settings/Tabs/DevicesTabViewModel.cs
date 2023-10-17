using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Device;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public class DevicesTabViewModel : RoutableScreen
{
    private readonly IDeviceVmFactory _deviceVmFactory;
    private readonly IDeviceService _deviceService;
    private readonly IWindowService _windowService;
    private bool _confirmedDisable;

    public DevicesTabViewModel(IDeviceService deviceService, IWindowService windowService, IDeviceVmFactory deviceVmFactory)
    {
        DisplayName = "Devices";

        _deviceService = deviceService;
        _windowService = windowService;
        _deviceVmFactory = deviceVmFactory;

        Devices = new ObservableCollection<DeviceSettingsViewModel>();
        this.WhenActivated(disposables =>
        {
            GetDevices();

            Observable.FromEventPattern<DeviceEventArgs>(x => _deviceService.DeviceAdded += x, x => _deviceService.DeviceAdded -= x)
                .Subscribe(d => AddDevice(d.EventArgs.Device))
                .DisposeWith(disposables);
            Observable.FromEventPattern<DeviceEventArgs>(x => _deviceService.DeviceRemoved += x, x => _deviceService.DeviceRemoved -= x)
                .Subscribe(d => RemoveDevice(d.EventArgs.Device))
                .DisposeWith(disposables);
        });
    }

    public ObservableCollection<DeviceSettingsViewModel> Devices { get; }

    public async Task<bool> ShowDeviceDisableDialog()
    {
        if (_confirmedDisable)
            return true;

        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Disabling device",
            "Disabling a device will cause it to stop updating. " +
            "\r\nSome SDKs will even go back to using manufacturer lighting (Artemis restart may be required)."
        );
        if (confirmed)
            _confirmedDisable = true;

        return confirmed;
    }

    private void GetDevices()
    {
        Devices.Clear();
        Dispatcher.UIThread.InvokeAsync(() => { Devices.AddRange(_deviceService.Devices.Select(d => _deviceVmFactory.DeviceSettingsViewModel(d, this))); }, DispatcherPriority.Background);
    }

    private void AddDevice(ArtemisDevice device)
    {
        // If the device was only enabled, don't add it
        if (Devices.Any(d => d.Device == device))
            return;

        Devices.Add(_deviceVmFactory.DeviceSettingsViewModel(device, this));
    }

    private void RemoveDevice(ArtemisDevice device)
    {
        // If the device was only disabled don't remove it
        if (_deviceService.Devices.Contains(device))
            return;

        DeviceSettingsViewModel? viewModel = Devices.FirstOrDefault(i => i.Device == device);
        if (viewModel != null)
            Devices.Remove(viewModel);
    }
}