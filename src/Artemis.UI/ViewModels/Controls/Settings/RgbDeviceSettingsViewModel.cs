using Humanizer;
using RGB.NET.Core;

namespace Artemis.UI.ViewModels.Controls.Settings
{
    public class RgbDeviceSettingsViewModel
    {
        public IRGBDevice Device { get; }

        public RgbDeviceSettingsViewModel(IRGBDevice device)
        {
            Device = device;

            Type = Device.DeviceInfo.DeviceType.ToString().Humanize();
            Name = Device.DeviceInfo.Model;
            Manufacturer = Device.DeviceInfo.Manufacturer;
            Enabled = true;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public bool Enabled { get; set; }
    }
}