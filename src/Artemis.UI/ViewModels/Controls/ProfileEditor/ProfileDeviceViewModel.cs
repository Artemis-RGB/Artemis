using System.Collections.Generic;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
    public class ProfileDeviceViewModel : PropertyChangedBase
    {
        private readonly List<ProfileLedViewModel> _leds;

        public ProfileDeviceViewModel(IRGBDevice device)
        {
            Device = device;
            _leds = new List<ProfileLedViewModel>();

            foreach (var led in Device)
                _leds.Add(new ProfileLedViewModel(led));
        }

        public IRGBDevice Device { get; }
        public IReadOnlyCollection<ProfileLedViewModel> Leds => _leds.AsReadOnly();

        public void Update()
        {
            foreach (var ledViewModel in _leds)
                ledViewModel.Update();
        }
    }
}