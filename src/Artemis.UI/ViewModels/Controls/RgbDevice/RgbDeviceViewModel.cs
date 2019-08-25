using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.ViewModels.Controls.RgbDevice
{
    public class RgbDeviceViewModel : PropertyChangedBase
    {
        private readonly List<RgbLedViewModel> _leds;
        private double _dragOffsetX;
        private double _dragOffsetY;

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

        public void StartMouseDrag(Point mouseStartPosition)
        {
            _dragOffsetX = Device.Location.X - mouseStartPosition.X;
            _dragOffsetY = Device.Location.Y - mouseStartPosition.Y;
        }

        public void UpdateMouseDrag(Point mousePosition)
        {
            var roundedX = Math.Round((mousePosition.X + _dragOffsetX) / 10, 0, MidpointRounding.AwayFromZero) * 10;
            var roundedY = Math.Round((mousePosition.Y + _dragOffsetY) / 10, 0, MidpointRounding.AwayFromZero) * 10;
            this.Device.Location = new RGB.NET.Core.Point(roundedX, roundedY);
        }

        public void FinishMouseDrag(Point mouseEndPosition)
        {
            // TODO: Save and update
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void MouseEnter()
        {
            if (SelectionStatus == SelectionStatus.None)
            {
                SelectionStatus = SelectionStatus.Hover;
                Cursor = Cursors.Hand;
            }
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void MouseLeave()
        {
            if (SelectionStatus == SelectionStatus.Hover)
            {
                SelectionStatus = SelectionStatus.None;
                Cursor = Cursors.Arrow;
            }
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