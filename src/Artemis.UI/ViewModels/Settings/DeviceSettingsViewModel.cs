using Humanizer;
using RGB.NET.Core;

namespace Artemis.UI.ViewModels.Settings
{
    public class DeviceSettingsViewModel
    {
        private readonly IRGBDevice _device;

        public DeviceSettingsViewModel(IRGBDevice device)
        {
            _device = device;

            Type = _device.DeviceInfo.DeviceType.ToString().Humanize();
            Name = _device.DeviceInfo.Model;
            Manufacturer = _device.DeviceInfo.Manufacturer;
            Enabled = true;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public bool Enabled { get; set; }
    }
}