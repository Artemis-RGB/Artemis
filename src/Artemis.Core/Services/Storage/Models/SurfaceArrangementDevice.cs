using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;

namespace Artemis.Core.Services.Models
{
    internal class SurfaceArrangementDevice
    {
        public SurfaceArrangementDevice? Anchor { get; }
        public RGBDeviceType DeviceType { get; }
        public ArrangementPosition Position { get; }

        public SurfaceArrangementDevice(SurfaceArrangementDevice? anchor, RGBDeviceType deviceType, ArrangementPosition position)
        {
            Anchor = anchor;
            DeviceType = deviceType;
            Position = position;
        }

        public void Apply(ArtemisSurface surface)
        {
            List<ArtemisDevice> devices = surface.Devices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == DeviceType).ToList();
            ArtemisDevice? previous = null;
            foreach (ArtemisDevice artemisDevice in devices)
            {
                if (previous != null)
                {

                } 
                previous = artemisDevice;
            }
        }
    }

    internal enum ArrangementPosition
    {
        Left,
        Right,
        Top,
        Bottom,
        Center
    }
}