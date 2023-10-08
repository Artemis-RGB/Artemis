using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Device;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using RGB.NET.Core;
using KeyboardLayoutType = Artemis.Core.KeyboardLayoutType;

namespace Artemis.UI.Services;

public class DeviceLayoutService : IDeviceLayoutService
{
    private readonly List<ArtemisDevice> _ignoredDevices;
    private readonly IDeviceService _deviceService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IWindowService _windowService;

    public DeviceLayoutService(IDeviceService deviceService, IMainWindowService mainWindowService, IWindowService windowService)
    {
        _deviceService = deviceService;
        _mainWindowService = mainWindowService;
        _windowService = windowService;
        _ignoredDevices = new List<ArtemisDevice>();

        deviceService.DeviceAdded += RgbServiceOnDeviceAdded;
        mainWindowService.MainWindowOpened += WindowServiceOnMainWindowOpened;
    }

    private async Task RequestLayoutInput(ArtemisDevice artemisDevice)
    {
        bool configure = await _windowService.ShowConfirmContentDialog(
            "Device requires layout info",
            $"Artemis could not detect the layout of your {artemisDevice.RgbDevice.DeviceInfo.DeviceName}. Please configure it manually",
            "Configure",
            "Ignore for now"
        );

        if (!configure)
        {
            _ignoredDevices.Add(artemisDevice);
            return;
        }

        if (!artemisDevice.DeviceProvider.CanDetectPhysicalLayout && !await DevicePhysicalLayoutDialogViewModel.SelectPhysicalLayout(_windowService, artemisDevice))
        {
            _ignoredDevices.Add(artemisDevice);
            return;
        }

        if (!artemisDevice.DeviceProvider.CanDetectLogicalLayout && !await DeviceLogicalLayoutDialogViewModel.SelectLogicalLayout(_windowService, artemisDevice))
        {
            _ignoredDevices.Add(artemisDevice);
            return;
        }

        await Task.Delay(400);
        _deviceService.SaveDevice(artemisDevice);
        _deviceService.ApplyDeviceLayout(artemisDevice, artemisDevice.GetBestDeviceLayout());
    }

    private bool DeviceNeedsLayout(ArtemisDevice d)
    {
        return d.DeviceType == RGBDeviceType.Keyboard &&
               (d.LogicalLayout == null || d.PhysicalLayout == KeyboardLayoutType.Unknown) &&
               (!d.DeviceProvider.CanDetectLogicalLayout || !d.DeviceProvider.CanDetectPhysicalLayout);
    }

    private async void WindowServiceOnMainWindowOpened(object? sender, EventArgs e)
    {
        List<ArtemisDevice> devices = _deviceService.Devices.Where(device => DeviceNeedsLayout(device) && !_ignoredDevices.Contains(device)).ToList();
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            foreach (ArtemisDevice artemisDevice in devices)
                await RequestLayoutInput(artemisDevice);
        });
    }

    private async void RgbServiceOnDeviceAdded(object? sender, DeviceEventArgs e)
    {
        if (_ignoredDevices.Contains(e.Device) || !DeviceNeedsLayout(e.Device) || !_mainWindowService.IsMainWindowOpen)
            return;

        await Dispatcher.UIThread.InvokeAsync(async () => await RequestLayoutInput(e.Device));
    }
}

public interface IDeviceLayoutService : IArtemisUIService
{
}