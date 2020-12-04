using System.Collections.Generic;
using RGB.NET.Core;

namespace Artemis.Core.Services.Models
{
    internal class SurfaceArrangement
    {
        public SurfaceArrangement()
        {
            Devices = new List<SurfaceArrangementDevice>();
        }

        public List<SurfaceArrangementDevice> Devices { get; }

        internal static SurfaceArrangement GetDefaultArrangement()
        {
            SurfaceArrangement arrangement = new SurfaceArrangement();
            arrangement.Devices.Add(new SurfaceArrangementDevice(null, RGBDeviceType.Keyboard, ArrangementPosition.Right));
            return arrangement;
        }
    }


}
