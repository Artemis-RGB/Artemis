using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Screens.Settings.Tabs.ViewModels;
using Artemis.UI.Avalonia.Shared;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using Humanizer;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Avalonia.Screens.Device.ViewModels
{
    public class DeviceSettingsViewModel : ActivatableViewModelBase
    {
        private readonly IDeviceService _deviceService;
        private readonly DevicesTabViewModel _devicesTabViewModel;
        private readonly IDeviceVmFactory _deviceVmFactory;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;

        public DeviceSettingsViewModel(ArtemisDevice device, DevicesTabViewModel devicesTabViewModel, IDeviceService deviceService, IWindowService windowService, IDeviceVmFactory deviceVmFactory,
            IRgbService rgbService)
        {
            _devicesTabViewModel = devicesTabViewModel;
            _deviceService = deviceService;
            _windowService = windowService;
            _deviceVmFactory = deviceVmFactory;
            _rgbService = rgbService;
            Device = device;

            Type = Device.DeviceType.ToString().Humanize();
            Name = Device.RgbDevice.DeviceInfo.Model;
            Manufacturer = Device.RgbDevice.DeviceInfo.Manufacturer;
        }

        public ArtemisDevice Device { get; }

        public string Type { get; }
        public string Name { get; }
        public string Manufacturer { get; }

        public bool CanDetectInput => Device.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Mouse;

        public bool IsDeviceEnabled
        {
            get => Device.IsEnabled;
            set => Task.Run(() => UpdateIsDeviceEnabled(value));
        }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void OpenPluginDirectory()
        {
            Utilities.OpenFolder(Device.DeviceProvider.Plugin.Directory.FullName);
        }

        public async Task DetectInput()
        {
            if (!CanDetectInput)
                return;

            await _windowService.CreateContentDialog()
                .WithViewModel<DeviceDetectInputViewModel>(out var viewModel, ("device", Device))
                .ShowAsync();

            if (viewModel.MadeChanges)
                _rgbService.SaveDevice(Device);
        }

        public async Task ViewProperties()
        {
            await _windowService.ShowDialogAsync(_deviceVmFactory.DevicePropertiesViewModel(Device));
        }

        private async Task UpdateIsDeviceEnabled(bool value)
        {
            if (!value)
                value = !await _devicesTabViewModel.ShowDeviceDisableDialog();

            if (value)
                _rgbService.EnableDevice(Device);
            else
                _rgbService.DisableDevice(Device);

            this.RaisePropertyChanged(nameof(IsDeviceEnabled));
            SaveDevice();
        }

        private void SaveDevice()
        {
            _rgbService.SaveDevice(Device);
        }
    }
}