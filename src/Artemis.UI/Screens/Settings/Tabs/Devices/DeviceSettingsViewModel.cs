using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.SurfaceEditor.Dialogs;
using Artemis.UI.Shared.Services;
using Humanizer;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsViewModel : Screen
    {
        private readonly IDeviceDebugVmFactory _deviceDebugVmFactory;
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private readonly IWindowManager _windowManager;

        public DeviceSettingsViewModel(ArtemisDevice device,
            IDeviceService deviceService,
            IDialogService dialogService,
            IWindowManager windowManager,
            IDeviceDebugVmFactory deviceDebugVmFactory,
            IRgbService rgbService)
        {
            _deviceService = deviceService;
            _dialogService = dialogService;
            _windowManager = windowManager;
            _deviceDebugVmFactory = deviceDebugVmFactory;
            _rgbService = rgbService;
            Device = device;

            Type = Device.RgbDevice.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;
        }

        public ArtemisDevice Device { get; }

        public string Type { get; }
        public string Name { get; }
        public string Manufacturer { get; }

        public bool CanDetectInput => Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard ||
                                      Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse;

        public bool IsDeviceEnabled
        {
            get => Device.IsEnabled;
            set { Task.Run(() => UpdateIsDeviceEnabled(value)); }
        }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void OpenPluginDirectory()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Device.DeviceProvider.Plugin.Directory.FullName);
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }

        public async Task DetectInput()
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceDetectInputViewModel>(
                new Dictionary<string, object> {{"device", Device}}
            );

            if ((bool) madeChanges)
                _rgbService.SaveDevice(Device);
        }

        public void ViewProperties()
        {
            _windowManager.ShowWindow(_deviceDebugVmFactory.DeviceDialogViewModel(Device));
        }
        private async Task UpdateIsDeviceEnabled(bool value)
        {
            if (!value)
                value = !await ((DeviceSettingsTabViewModel) Parent).ShowDeviceDisableDialog();

            if (value)
                _rgbService.EnableDevice(Device);
            else
                _rgbService.DisableDevice(Device);
            NotifyOfPropertyChange(nameof(IsDeviceEnabled));
            SaveDevice();
        }

        private void SaveDevice()
        {
            _rgbService.SaveDevice(Device);
        }
    }
}