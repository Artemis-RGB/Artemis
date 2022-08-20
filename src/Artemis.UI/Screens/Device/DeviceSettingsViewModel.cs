using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using Humanizer;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Device
{
    public class DeviceSettingsViewModel : ActivatableViewModelBase
    {
        private readonly IDeviceService _deviceService;
        private readonly DevicesTabViewModel _devicesTabViewModel;
        private readonly IDeviceVmFactory _deviceVmFactory;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;
        private bool _togglingDevice;

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

            DetectInput = ReactiveCommand.CreateFromTask(ExecuteDetectInput, this.WhenAnyValue(vm => vm.CanDetectInput));
        }

        public ArtemisDevice Device { get; }

        public string Type { get; }
        public string Name { get; }
        public string Manufacturer { get; }

        public bool CanDetectInput => Device.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Mouse;
        public ReactiveCommand<Unit, Unit> DetectInput { get; }

        public bool IsDeviceEnabled
        {
            get => Device.IsEnabled;
            set => Dispatcher.UIThread.InvokeAsync(async () => await UpdateIsDeviceEnabled(value));
        }

        public bool TogglingDevice
        {
            get => _togglingDevice;
            set => RaiseAndSetIfChanged(ref _togglingDevice, value);
        }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void OpenPluginDirectory()
        {
            Utilities.OpenFolder(Device.DeviceProvider.Plugin.Directory.FullName);
        }

        private async Task ExecuteDetectInput()
        {
            if (!CanDetectInput)
                return;

            await _windowService.CreateContentDialog()
                .WithTitle($"{Device.RgbDevice.DeviceInfo.DeviceName} - Detect input")
                .WithViewModel<DeviceDetectInputViewModel>(out DeviceDetectInputViewModel? viewModel, ("device", Device))
                .WithCloseButtonText("Cancel")
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
            if (TogglingDevice)
                return;

            try
            {
                TogglingDevice = true;

                if (!value)
                    value = !await _devicesTabViewModel.ShowDeviceDisableDialog();

                if (value)
                    _rgbService.EnableDevice(Device);
                else
                    _rgbService.DisableDevice(Device);

                this.RaisePropertyChanged(nameof(IsDeviceEnabled));
                SaveDevice();
            }
            finally
            {
                TogglingDevice = false;
            }
        }

        private void SaveDevice()
        {
            _rgbService.SaveDevice(Device);
        }
    }
}