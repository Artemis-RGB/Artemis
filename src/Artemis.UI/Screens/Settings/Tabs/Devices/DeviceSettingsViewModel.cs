using System;
using System.Diagnostics;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Humanizer;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsViewModel
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;

        public DeviceSettingsViewModel(ArtemisDevice device, IDeviceService deviceService, IDialogService dialogService)
        {
            _deviceService = deviceService;
            _dialogService = dialogService;
            Device = device;

            Type = Device.RgbDevice.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;
            IsDeviceEnabled = true;
        }

        public ArtemisDevice Device { get; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public bool IsDeviceEnabled { get; set; }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void ShowDeviceDebugger()
        {
        }

        public async void OpenPluginDirectory()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Device.Plugin.PluginInfo.Directory.FullName);
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }
    }
}