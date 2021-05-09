using RGB.NET.Core;

namespace Artemis.Core
{
    internal class DeviceAdaptionHint : IAdaptionHint
    {
        public RGBDeviceType DeviceType { get; set; }
        
        public bool ApplyToAllMatches { get; set; }
        public int Skip { get; set; }
        public int Amount { get; set; }
    }
}