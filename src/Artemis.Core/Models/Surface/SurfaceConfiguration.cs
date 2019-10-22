using System.Collections.Generic;
using Artemis.Storage.Entities;

namespace Artemis.Core.Models.Surface
{
    public class SurfaceConfiguration
    {
        internal SurfaceConfiguration(string name)
        {
            SurfaceEntity = new SurfaceEntity {SurfacePositions = new List<SurfacePositionEntity>()};
            Guid = System.Guid.NewGuid().ToString();

            Name = name;
            IsActive = false;
            DeviceConfigurations = new List<SurfaceDeviceConfiguration>();

            ApplyToEntity();
        }

        internal SurfaceConfiguration(SurfaceEntity surfaceEntity)
        {
            SurfaceEntity = surfaceEntity;
            Guid = surfaceEntity.Guid;

            Name = surfaceEntity.Name;
            IsActive = surfaceEntity.IsActive;
            DeviceConfigurations = new List<SurfaceDeviceConfiguration>();

            if (surfaceEntity.SurfacePositions == null)
                return;
            foreach (var position in surfaceEntity.SurfacePositions)
                DeviceConfigurations.Add(new SurfaceDeviceConfiguration(position, this));
        }

        internal SurfaceEntity SurfaceEntity { get; set; }
        internal string Guid { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; internal set; }
        public List<SurfaceDeviceConfiguration> DeviceConfigurations { get; internal set; }

        internal void ApplyToEntity()
        {
            SurfaceEntity.Guid = Guid;
            SurfaceEntity.Name = Name;
            SurfaceEntity.IsActive = IsActive;
        }

        internal void Destroy()
        {
            SurfaceEntity = null;

            foreach (var deviceConfiguration in DeviceConfigurations)
                deviceConfiguration.Destroy();
            DeviceConfigurations.Clear();
        }
    }
}