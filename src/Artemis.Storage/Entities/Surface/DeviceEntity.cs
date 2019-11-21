using System;

namespace Artemis.Storage.Entities.Surface
{
    public class DeviceEntity
    {
        public Guid Id { get; set; }

        public int DeviceHashCode { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Rotation { get; set; }
        public double Scale { get; set; }
        public int ZIndex { get; set; }
    }
}