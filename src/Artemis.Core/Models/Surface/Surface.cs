using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;

namespace Artemis.Core.Models.Surface
{
    public class Surface
    {
        internal Surface(RGBSurface rgbSurface, string name, double scale)
        {
            SurfaceEntity = new SurfaceEntity {DeviceEntities = new List<DeviceEntity>()};
            EntityId = Guid.NewGuid();

            Name = name;
            Scale = scale;
            RgbSurface = rgbSurface;
            IsActive = false;

            // Devices are not populated here but as they are detected
            Devices = new List<Device>();

            ApplyToEntity();
        }

        internal Surface(RGBSurface rgbSurface, SurfaceEntity surfaceEntity, double scale)
        {
            SurfaceEntity = surfaceEntity;
            EntityId = surfaceEntity.Id;

            RgbSurface = rgbSurface;
            Scale = scale;
            Name = surfaceEntity.Name;
            IsActive = surfaceEntity.IsActive;

            // Devices are not populated here but as they are detected
            Devices = new List<Device>(); 
        }

        public RGBSurface RgbSurface { get; }
        public double Scale { get; private set; }
        public string Name { get; set; }
        public bool IsActive { get; internal set; }
        public List<Device> Devices { get; internal set; }

        internal SurfaceEntity SurfaceEntity { get; set; }
        internal Guid EntityId { get; set; }

        internal void ApplyToEntity()
        {
            SurfaceEntity.Id = EntityId;
            SurfaceEntity.Name = Name;
            SurfaceEntity.IsActive = IsActive;

            // Add missing device entities, don't remove old ones in case they come back later
            foreach (var deviceEntity in Devices.Select(d => d.DeviceEntity).ToList())
            {
                if (!SurfaceEntity.DeviceEntities.Contains(deviceEntity))
                    SurfaceEntity.DeviceEntities.Add(deviceEntity);
            }
        }

        public void UpdateScale(double value)
        {
            Scale = value;
            foreach (var device in Devices)
                device.CalculateRenderRectangle();

            OnScaleChanged();
        }

        #region Events

        public event EventHandler<EventArgs> ScaleChanged;

        protected virtual void OnScaleChanged()
        {
            ScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}