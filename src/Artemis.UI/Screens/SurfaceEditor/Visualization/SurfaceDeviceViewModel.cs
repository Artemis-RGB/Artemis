using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using RGB.NET.Core;
using SkiaSharp;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.Screens.SurfaceEditor.Visualization
{
    public class SurfaceDeviceViewModel : PropertyChangedBase
    {
        private readonly IRgbService _rgbService;
        private Cursor _cursor;
        private double _dragOffsetX;
        private double _dragOffsetY;
        private SelectionStatus _selectionStatus;

        public SurfaceDeviceViewModel(ArtemisDevice device, IRgbService rgbService)
        {
            Device = device;
            _rgbService = rgbService;
        }

        public ArtemisDevice Device { get; }

        public SelectionStatus SelectionStatus
        {
            get => _selectionStatus;
            set => SetAndNotify(ref _selectionStatus, value);
        }

        public bool CanDetectInput => Device.DeviceType == RGBDeviceType.Keyboard ||
                                      Device.DeviceType == RGBDeviceType.Mouse;

        public Cursor Cursor
        {
            get => _cursor;
            set => SetAndNotify(ref _cursor, value);
        }
        
        public void StartMouseDrag(Point mouseStartPosition)
        {
            _dragOffsetX = Device.X - mouseStartPosition.X;
            _dragOffsetY = Device.Y - mouseStartPosition.Y;
        }

        public void UpdateMouseDrag(Point mousePosition)
        {
            float roundedX = (float) Math.Round((mousePosition.X + _dragOffsetX) / 10d, 0, MidpointRounding.AwayFromZero) * 10f;
            float roundedY = (float) Math.Round((mousePosition.Y + _dragOffsetY) / 10d, 0, MidpointRounding.AwayFromZero) * 10f;

            if (Fits(roundedX, roundedY))
            {
                Device.X = roundedX;
                Device.Y = roundedY;
            }
            else if (Fits(roundedX, (float) Device.Y))
            {
                Device.X = roundedX;
            }
            else if (Fits((float) Device.X, roundedY))
            {
                Device.Y = roundedY;
            }
        }


        // ReSharper disable once UnusedMember.Global - Called from view
        public void MouseEnter(object sender, MouseEventArgs e)
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

        public MouseDevicePosition GetMouseDevicePosition(Point position)
        {
            if ((new Point(0, 0) - position).LengthSquared < 5) return MouseDevicePosition.TopLeft;

            return MouseDevicePosition.Regular;
        }

        private bool Fits(float x, float y)
        {
            if (x < 0 || y < 0)
                return false;

            List<SKRect> own = Device.Leds
                .Select(l => SKRect.Create(l.Rectangle.Left + x, l.Rectangle.Top + y, l.Rectangle.Width, l.Rectangle.Height))
                .ToList();
            List<SKRect> others = _rgbService.EnabledDevices
                .Where(d => d != Device && d.IsEnabled)
                .SelectMany(d => d.Leds)
                .Select(l => SKRect.Create(l.Rectangle.Left + (float) l.Device.X, l.Rectangle.Top + (float) l.Device.Y, l.Rectangle.Width, l.Rectangle.Height))
                .ToList();

            return !own.Any(o => others.Any(l => l.IntersectsWith(o)));
        }
    }

    public enum MouseDevicePosition
    {
        Regular,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    
    public enum SelectionStatus
    {
        None,
        Hover,
        Selected
    }
}