using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.SurfaceEditor.Dialogs;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared.Services;
using Humanizer;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsViewModel : PropertyChangedBase
    {
        private readonly IDeviceDebugVmFactory _deviceDebugVmFactory;
        private readonly ISurfaceService _surfaceService;
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        private bool _isDeviceEnabled;

        public DeviceSettingsViewModel(ArtemisDevice device,
            IDeviceService deviceService,
            IDialogService dialogService,
            IWindowManager windowManager,
            IDeviceDebugVmFactory deviceDebugVmFactory,
            ISurfaceService surfaceService)
        {
            _deviceService = deviceService;
            _dialogService = dialogService;
            _windowManager = windowManager;
            _deviceDebugVmFactory = deviceDebugVmFactory;
            _surfaceService = surfaceService;
            Device = device;

            Type = Device.RgbDevice.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;

            // TODO: Implement this bad boy
            IsDeviceEnabled = true;
        }

        public ArtemisDevice Device { get; }

        public string Type { get; }
        public string Name { get; }
        public string Manufacturer { get; }

        public bool CanDetectInput => Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard ||
                                      Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse;

        public bool IsDeviceEnabled
        {
            get => _isDeviceEnabled;
            set => SetAndNotify(ref _isDeviceEnabled, value);
        }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void ShowDeviceDebugger()
        {
            _windowManager.ShowWindow(_deviceDebugVmFactory.Create(Device));
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
                new Dictionary<string, object> { { "device", Device } }
            );

            if ((bool)madeChanges)
                _surfaceService.UpdateSurfaceConfiguration(_surfaceService.ActiveSurface, true);
        }

        public async Task ViewProperties()
        {
            object madeChanges = await _dialogService.ShowDialog<SurfaceDeviceConfigViewModel>(
                new Dictionary<string, object> {{"device", Device}}
            );

            if ((bool) madeChanges)
                _surfaceService.UpdateSurfaceConfiguration(_surfaceService.ActiveSurface, true);
        }
    }
}