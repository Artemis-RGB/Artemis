using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.RgbDevice
{
    public class RgbDeviceViewModel : PropertyChangedBase
    {
        private readonly List<RgbLedViewModel> _leds;

        public RgbDeviceViewModel(IRGBDevice device)
        {
            Device = device;
            _leds = new List<RgbLedViewModel>();

            foreach (var led in Device)
                _leds.Add(new RgbLedViewModel(led));
        }

        public Cursor Cursor { get; set; }
        public SelectionStatus SelectionStatus { get; set; }

        public IRGBDevice Device { get; }
        public IReadOnlyCollection<RgbLedViewModel> Leds => _leds.AsReadOnly();

        public void Update()
        {
            foreach (var rgbLedViewModel in _leds)
                rgbLedViewModel.Update();
        }

        public void SetColorsEnabled(bool enabled)
        {
            foreach (var rgbLedViewModel in _leds)
                rgbLedViewModel.ColorsEnabled = enabled;
        }

        public Rect DeviceRectangle => new Rect(Device.Location.X, Device.Location.Y, Device.Size.Width, Device.Size.Height);
    }

    public enum SelectionStatus
    {
        None,
        Hover,
        Selected
    }
}