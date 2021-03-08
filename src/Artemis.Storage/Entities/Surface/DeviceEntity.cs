using System.Collections.Generic;

namespace Artemis.Storage.Entities.Surface
{
    public class DeviceEntity
    {
        public DeviceEntity()
        {
            InputIdentifiers = new List<DeviceInputIdentifierEntity>();
        }
        
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public int ZIndex { get; set; }
        public float RedScale { get; set; }
        public float GreenScale { get; set; }
        public float BlueScale { get; set; }
        public bool IsEnabled { get; set; }

        public int PhysicalLayout { get; set; }
        public string LogicalLayout { get; set; }
        public string CustomLayoutPath { get; set; }

        public List<DeviceInputIdentifierEntity> InputIdentifiers { get; set; }
        
    }

    public class DeviceInputIdentifierEntity
    {
        public string InputProvider { get; set; }
        public object Identifier { get; set; }
    }
}