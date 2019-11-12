using System.Collections.Generic;
using System.Windows;
using Artemis.Core.Models.Surface;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
    public class ProfileDeviceViewModel : PropertyChangedBase
    {
        private readonly List<ProfileLedViewModel> _leds;

        public ProfileDeviceViewModel(Device device)
        {
            Device = device;
            _leds = new List<ProfileLedViewModel>();

            if (Device.RgbDevice != null)
            {
                foreach (var led in Device.RgbDevice)
                    _leds.Add(new ProfileLedViewModel(led));
            }
        }

        public Device Device { get; set; }

        public double X
        {
            get => Device.X;
            set => Device.X = value;
        }

        public double Y
        {
            get => Device.Y;
            set => Device.Y = value;
        }

        public int ZIndex
        {
            get => Device.ZIndex;
            set => Device.ZIndex = value;
        }

        public IReadOnlyCollection<ProfileLedViewModel> Leds => _leds.AsReadOnly();

        public Rect DeviceRectangle => Device.RgbDevice == null
            ? new Rect()
            : new Rect(X, Y, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height);

        public void Update()
        {
            foreach (var ledViewModel in _leds)
                ledViewModel.Update();
        }
    }
}