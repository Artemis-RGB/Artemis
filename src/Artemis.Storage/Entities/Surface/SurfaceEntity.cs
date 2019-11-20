using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Surface
{
    public class SurfaceEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }

        public List<DeviceEntity> DeviceEntities { get; set; }
    }
}