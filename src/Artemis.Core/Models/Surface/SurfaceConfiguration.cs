using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities;
using RGB.NET.Core;

namespace Artemis.Core.Models.Surface
{
    public class SurfaceConfiguration
    {
        internal SurfaceConfiguration()
        {
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
        public bool IsActive { get; internal set; }
        public List<SurfaceDeviceConfiguration> DeviceConfigurations { get; internal set; }
    }
}