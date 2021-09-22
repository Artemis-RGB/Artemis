using System.Collections.Generic;

namespace Artemis.Storage.Entities.Surface
{
    public class DeviceEntity
    {
        public DeviceEntity()
        {
            InputIdentifiers = new List<DeviceInputIdentifierEntity>();
            InputMappings = new List<InputMappingEntity>();
            Categories = new List<int>();
        }

        public string Id { get; set; }
        public string DeviceProvider { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public int ZIndex { get; set; }
        public float RedScale { get; set; }
        public float GreenScale { get; set; }
        public float BlueScale { get; set; }
        public bool IsEnabled { get; set; }

        public bool DisableDefaultLayout { get; set; }
        public int PhysicalLayout { get; set; }
        public string LogicalLayout { get; set; }
        public string CustomLayoutPath { get; set; }

        public List<DeviceInputIdentifierEntity> InputIdentifiers { get; set; }
        public List<InputMappingEntity> InputMappings { get; set; }
        public List<int> Categories { get; set; }
    }

    public class InputMappingEntity
    {
        public int OriginalLedId { get; set; }
        public int MappedLedId { get; set; }
    }

    public class DeviceInputIdentifierEntity
    {
        public string InputProvider { get; set; }
        public object Identifier { get; set; }
    }
}