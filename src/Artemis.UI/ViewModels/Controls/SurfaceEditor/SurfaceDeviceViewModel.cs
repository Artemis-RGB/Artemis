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

        public SurfaceDeviceViewModel(SurfaceDeviceConfiguration deviceConfiguration)
        {
            DeviceConfiguration = deviceConfiguration;
            _leds = new List<SurfaceLedViewModel>();

            if (DeviceConfiguration.Device != null)
            {
                foreach (var led in DeviceConfiguration.Device)
                    _leds.Add(new SurfaceLedViewModel(led));
            }
        }

        public SurfaceDeviceConfiguration DeviceConfiguration { get; }
        public SelectionStatus SelectionStatus { get; set; }
        public Cursor Cursor { get; set; }

        public double X
        {
            get => DeviceConfiguration.X;
            set => DeviceConfiguration.X = value;
        }

        public double Y
        {
            get => DeviceConfiguration.Y;
            set => DeviceConfiguration.Y = value;
        }

        public int ZIndex
        {
            get => DeviceConfiguration.ZIndex;
            set => DeviceConfiguration.ZIndex = value;
        }

        public IReadOnlyCollection<SurfaceLedViewModel> Leds => _leds.AsReadOnly();

        public Rect DeviceRectangle => new Rect(X, Y, DeviceConfiguration.Device.Size.Width, DeviceConfiguration.Device.Size.Height);

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