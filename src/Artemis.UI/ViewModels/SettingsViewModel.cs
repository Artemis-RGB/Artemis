using System.Linq;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class SettingsViewModel : Screen, ISettingsViewModel
    {
        private readonly IRgbService _rgbService;

        public SettingsViewModel(IRgbService rgbService)
        {
            _rgbService = rgbService;
            _rgbService.FinishedLoadedDevices += (sender, args) => SetTestDevice();
        }

        protected override void OnActivate()
        {
            SetTestDevice();
            base.OnActivate();
        }

        private void SetTestDevice()
        {
            if (!IsActive)
                return;

            if (!_rgbService.LoadingDevices)
                TestDevice = _rgbService.Surface.Devices.FirstOrDefault(d => d.DeviceInfo.DeviceType == RGBDeviceType.Keyboard);
        }

        public IRGBDevice TestDevice { get; set; }
        public string Title => "Settings";
    }
}