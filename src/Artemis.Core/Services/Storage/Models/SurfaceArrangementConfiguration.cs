using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;

namespace Artemis.Core.Services.Models
{
    internal class SurfaceArrangementConfiguration
    {
        public SurfaceArrangementConfiguration(SurfaceArrangementType? anchor, HorizontalArrangementPosition horizontalPosition, VerticalArrangementPosition verticalPosition,
            int margin)
        {
            Anchor = anchor;
            HorizontalPosition = horizontalPosition;
            VerticalPosition = verticalPosition;

            MarginLeft = margin;
            MarginTop = margin;
            MarginRight = margin;
            MarginBottom = margin;

            SurfaceArrangement = null!;
        }


        public SurfaceArrangementType? Anchor { get; }
        public HorizontalArrangementPosition HorizontalPosition { get; }
        public VerticalArrangementPosition VerticalPosition { get; }

        public int MarginLeft { get; }
        public int MarginTop { get; }
        public int MarginRight { get; }
        public int MarginBottom { get; }
        public SurfaceArrangement SurfaceArrangement { get; set; }

        public bool Apply(List<ArtemisDevice> devices)
        {
            if (Anchor != null && !Anchor.HasDevices(devices))
                return false;

            // Start at the edge of the anchor, if there is no anchor start at any device
            Point startPoint = Anchor?.GetEdge(HorizontalPosition, VerticalPosition) ??
                               new SurfaceArrangementType(SurfaceArrangement, RGBDeviceType.All, 1).GetEdge(HorizontalPosition, VerticalPosition);

            // Stack multiple devices of the same type vertically if they are wider than they are tall
            bool stackVertically = devices.Average(d => d.RgbDevice.Size.Width) >= devices.Average(d => d.RgbDevice.Size.Height);

            ArtemisDevice? previous = null;
            foreach (ArtemisDevice artemisDevice in devices)
            {
                if (previous != null)
                {
                    if (stackVertically)
                    {
                        artemisDevice.X = previous.X;
                        artemisDevice.Y = previous.RgbDevice.Location.Y + previous.RgbDevice.Size.Height + MarginTop / 2.0;
                    }
                    else
                    {
                        artemisDevice.X = previous.RgbDevice.Location.X + previous.RgbDevice.Size.Width + MarginLeft / 2.0;
                        artemisDevice.Y = previous.Y;
                    }
                }
                else
                {
                    artemisDevice.X = HorizontalPosition switch
                    {
                        HorizontalArrangementPosition.Left => startPoint.X - artemisDevice.RgbDevice.Size.Width - MarginRight,
                        HorizontalArrangementPosition.Right => startPoint.X + MarginLeft,
                        HorizontalArrangementPosition.Center => startPoint.X - artemisDevice.RgbDevice.Size.Width / 2,
                        HorizontalArrangementPosition.Equal => startPoint.X,
                        _ => artemisDevice.X
                    };
                    artemisDevice.Y = VerticalPosition switch
                    {
                        VerticalArrangementPosition.Top => startPoint.Y - artemisDevice.RgbDevice.Size.Height - MarginBottom,
                        VerticalArrangementPosition.Bottom => startPoint.Y + MarginTop,
                        VerticalArrangementPosition.Center => startPoint.Y - artemisDevice.RgbDevice.Size.Height / 2,
                        VerticalArrangementPosition.Equal => startPoint.Y,
                        _ => artemisDevice.X
                    };
                }

                artemisDevice.ApplyToRgbDevice();
                previous = artemisDevice;

                SurfaceArrangement.ArrangedDevices.Add(artemisDevice);
            }

            return true;
        }
    }
}