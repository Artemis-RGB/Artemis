using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Controls.Settings;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class SettingsViewModel : Screen, ISettingsViewModel
    {
        public SettingsViewModel(IRgbService rgbService)
        {
            DeviceSettingsViewModels = new BindableCollection<RgbDeviceSettingsViewModel>();
            foreach (var device in rgbService.Surface.Devices)
                DeviceSettingsViewModels.Add(new RgbDeviceSettingsViewModel(device));

            rgbService.DeviceLoaded += UpdateDevices;
        }

        public BindableCollection<RgbDeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public string Title => "Settings";

        private void UpdateDevices(object sender, DeviceEventArgs deviceEventArgs)
        {
            DeviceSettingsViewModels.Add(new RgbDeviceSettingsViewModel(deviceEventArgs.Device));
        }
    }
}