using System.Collections.Generic;
using Artemis.Storage.Entities;

namespace Artemis.Core.Models.Surface
{
    public class SurfaceConfiguration
    {
        public SurfaceConfiguration(string name)
        {
            Name = name;
            DeviceConfigurations = new List<SurfaceDeviceConfiguration>();
        }

        internal SurfaceConfiguration(SurfaceEntity surfaceEntity)
        {
            Guid = surfaceEntity.Guid;
            Name = surfaceEntity.Name;
            IsActive = surfaceEntity.IsActive;
            DeviceConfigurations = new List<SurfaceDeviceConfiguration>();

            if (surfaceEntity.SurfacePositions == null)
                return;
            foreach (var position in surfaceEntity.SurfacePositions)
                DeviceConfigurations.Add(new SurfaceDeviceConfiguration(position, this));
        }

        internal string Guid { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<SurfaceDeviceConfiguration> DeviceConfigurations { get; set; }
    }
}