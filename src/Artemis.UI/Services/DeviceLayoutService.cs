using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Settings.Device;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using RGB.NET.Core;
using RGB.NET.Layout;
using KeyboardLayoutType = Artemis.Core.KeyboardLayoutType;

namespace Artemis.UI.Services
{
    public class DeviceLayoutService : IDeviceLayoutService
    {
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;
        private readonly IMessageService _messageService;
        private readonly List<ArtemisDevice> _ignoredDevices;

        public DeviceLayoutService(IDialogService dialogService, IRgbService rgbService, IWindowService windowService, IMessageService messageService)
        {
            _dialogService = dialogService;
            _rgbService = rgbService;
            _windowService = windowService;
            _messageService = messageService;
            _ignoredDevices = new List<ArtemisDevice>();

            rgbService.DeviceAdded += RgbServiceOnDeviceAdded;
            windowService.MainWindowOpened += async (_, _) => await RequestLayoutInput();
        }

        private async Task RequestLayoutInput()
        {
            List<ArtemisDevice> devices = _rgbService.Devices.Where(device => DeviceNeedsLayout(device) && !_ignoredDevices.Contains(device)).ToList();
            foreach (ArtemisDevice artemisDevice in devices)
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
                    continue;
                }

                await _dialogService.ShowDialog<DeviceLayoutDialogViewModel>(new Dictionary<string, object> {{"device", artemisDevice}});
            }
        }

        private void RgbServiceOnDeviceAdded(object sender, DeviceEventArgs e)
        {
            if (!DeviceNeedsLayout(e.Device))
                return;

            if (!_windowService.IsMainWindowOpen)
            {
                _messageService.ShowNotification("New device detected", "Detected a new device that needs layout setup", PackIconKind.Keyboard);
                return;
            }
        }

        private bool DeviceNeedsLayout(ArtemisDevice d) => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard &&
                                                           d.LogicalLayout == null ||
                                                           d.PhysicalLayout == KeyboardLayoutType.Unknown &&
                                                           (!d.DeviceProvider.CanDetectLogicalLayout || !d.DeviceProvider.CanDetectPhysicalLayout);
    }

    public interface IDeviceLayoutService : IArtemisUIService
    {
    }
}