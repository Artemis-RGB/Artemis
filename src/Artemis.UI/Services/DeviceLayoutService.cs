using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Settings.Device;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using RGB.NET.Core;
using KeyboardLayoutType = Artemis.Core.KeyboardLayoutType;

namespace Artemis.UI.Services
{
    public class DeviceLayoutService : IDeviceLayoutService
    {
        private readonly IDialogService _dialogService;
        private readonly List<ArtemisDevice> _ignoredDevices;
        private readonly IMessageService _messageService;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;

        public DeviceLayoutService(IDialogService dialogService, IRgbService rgbService, IWindowService windowService, IMessageService messageService)
        {
            _dialogService = dialogService;
            _rgbService = rgbService;
            _windowService = windowService;
            _messageService = messageService;
            _ignoredDevices = new List<ArtemisDevice>();

            rgbService.DeviceAdded += RgbServiceOnDeviceAdded;
            windowService.MainWindowOpened += WindowServiceOnMainWindowOpened;
        }

        private async Task RequestLayoutInput(ArtemisDevice artemisDevice)
        {
            bool configure = await _dialogService.ShowConfirmDialog(
                "Device requires layout info",
                $"Artemis could not detect the layout of your {artemisDevice.RgbDevice.DeviceInfo.DeviceName}. Please configure out manually",
                "Configure",
                "Ignore for now"
            );

            if (!configure)
            {
                _ignoredDevices.Add(artemisDevice);
                return;
            }

            await _dialogService.ShowDialog<DeviceLayoutDialogViewModel>(new Dictionary<string, object> {{"device", artemisDevice}});
        }

        private bool DeviceNeedsLayout(ArtemisDevice d)
        {
            return d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard &&
                   (d.LogicalLayout == null || d.PhysicalLayout == KeyboardLayoutType.Unknown) &&
                   (!d.DeviceProvider.CanDetectLogicalLayout || !d.DeviceProvider.CanDetectPhysicalLayout);
        }

        #region Event handlers

        private async void WindowServiceOnMainWindowOpened(object? sender, EventArgs e)
        {
            List<ArtemisDevice> devices = _rgbService.Devices.Where(device => DeviceNeedsLayout(device) && !_ignoredDevices.Contains(device)).ToList();
            foreach (ArtemisDevice artemisDevice in devices)
                await RequestLayoutInput(artemisDevice);
        }

        private async void RgbServiceOnDeviceAdded(object sender, DeviceEventArgs e)
        {
            if (_ignoredDevices.Contains(e.Device) || !DeviceNeedsLayout(e.Device))
                return;

            if (!_windowService.IsMainWindowOpen)
            {
                _messageService.ShowNotification("New device detected", "Detected a new device that needs layout setup", PackIconKind.Keyboard);
                return;
            }

            await RequestLayoutInput(e.Device);
        }

        #endregion
    }

    public interface IDeviceLayoutService : IArtemisUIService
    {
    }
}