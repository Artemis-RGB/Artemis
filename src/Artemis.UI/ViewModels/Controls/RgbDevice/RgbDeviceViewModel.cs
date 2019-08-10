using System.Collections.Generic;
using RGB.NET.Core;

namespace Artemis.UI.ViewModels.Controls.RgbDevice
{
    public class RgbDeviceViewModel
    {
        private readonly List<RgbLedViewModel> _leds;

        public RgbDeviceViewModel(IRGBDevice device)
        {
            Device = device;
            _leds = new List<RgbLedViewModel>();

            foreach (var led in Device)
                _leds.Add(new RgbLedViewModel(led));
        }

        public IRGBDevice Device { get; }
        public IReadOnlyCollection<RgbLedViewModel> Leds => _leds.AsReadOnly();

        public void Update()
        {
            foreach (var rgbLedViewModel in _leds)
                rgbLedViewModel.Update();
        }
    }
}