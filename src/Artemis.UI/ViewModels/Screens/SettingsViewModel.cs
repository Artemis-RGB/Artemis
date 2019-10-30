using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Controls.Settings;
using Artemis.UI.ViewModels.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class SettingsViewModel : Screen, ISettingsViewModel
    {
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel, IRgbService rgbService, IWindowManager windowManager, ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            DeviceSettingsViewModels = new BindableCollection<RgbDeviceSettingsViewModel>();
            foreach (var device in rgbService.LoadedDevices)
                DeviceSettingsViewModels.Add(new RgbDeviceSettingsViewModel(device));

            rgbService.DeviceLoaded += UpdateDevices;
        }

        public BindableCollection<RgbDeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public string Title => "Settings";

        public void ShowDebugger()
        {
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
        }

        private void UpdateDevices(object sender, DeviceEventArgs deviceEventArgs)
        {
            DeviceSettingsViewModels.Add(new RgbDeviceSettingsViewModel(deviceEventArgs.Device));
        }
    }
}