using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.ViewModels.Controls.SurfaceEditor
{
    public class SurfaceDeviceViewModel : PropertyChangedBase
    {
        private double _dragOffsetX;
        private double _dragOffsetY;
        private readonly List<SurfaceLedViewModel> _leds;

        public SurfaceDeviceViewModel(Device device)
        {
            Device = device;
            _leds = new List<SurfaceLedViewModel>();

            if (Device.RgbDevice != null)
            {
                foreach (var led in Device.RgbDevice)
                    _leds.Add(new SurfaceLedViewModel(led));
            }
        }

        public Device Device { get; set; }
        public SelectionStatus SelectionStatus { get; set; }
        public Cursor Cursor { get; set; }

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

        public IReadOnlyCollection<SurfaceLedViewModel> Leds => _leds.AsReadOnly();

        public Rect DeviceRectangle => Device.RgbDevice == null
            ? new Rect()
            : new Rect(X, Y, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height);

        public void StartMouseDrag(Point mouseStartPosition)
        {
            _dragOffsetX = X - mouseStartPosition.X;
            _dragOffsetY = Y - mouseStartPosition.Y;
        }

        public void UpdateMouseDrag(Point mousePosition)
        {
            var roundedX = Math.Round((mousePosition.X + _dragOffsetX) / 10, 0, MidpointRounding.AwayFromZero) * 10;
            var roundedY = Math.Round((mousePosition.Y + _dragOffsetY) / 10, 0, MidpointRounding.AwayFromZero) * 10;
            X = Math.Max(0, roundedX);
            Y = Math.Max(0, roundedY);
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
    }

    public enum SelectionStatus
    {
        None,
        Hover,
        Selected
    }
}