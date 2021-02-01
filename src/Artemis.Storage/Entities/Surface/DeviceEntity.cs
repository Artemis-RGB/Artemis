using System.Collections.Generic;

namespace Artemis.Storage.Entities.Surface
{
    public class DeviceEntity
    {
        public DeviceEntity()
        {
            InputIdentifiers = new List<DeviceInputIdentifierEntity>();
        }

        public string DeviceIdentifier { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }
        public double Scale { get; set; }
        public int ZIndex { get; set; }
        public double RedScale { get; set; }
        public double GreenScale { get; set; }
        public double BlueScale { get; set; }
        public bool IsEnabled { get; set; }

        public List<DeviceInputIdentifierEntity> InputIdentifiers { get; set; }
    }

    public class DeviceInputIdentifierEntity
    {
        public string InputProvider { get; set; }
        public object Identifier { get; set; }
    }
}