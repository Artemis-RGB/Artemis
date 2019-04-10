using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels.Settings
{
    public class SettingsViewModel : Screen, ISettingsViewModel
    {
        public SettingsViewModel(IRgbService rgbService)
        {
            DeviceSettingsViewModels = new List<DeviceSettingsViewModel>();
            foreach (var device in rgbService.Surface.Devices)
                DeviceSettingsViewModels.Add(new DeviceSettingsViewModel(device));

            rgbService.DeviceLoaded += UpdateDevices;
        }

        public List<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public string Title => "Settings";

        private void UpdateDevices(object sender, DeviceEventArgs deviceEventArgs)
        {
            DeviceSettingsViewModels.Add(new DeviceSettingsViewModel(deviceEventArgs.Device));
        }
    }
}