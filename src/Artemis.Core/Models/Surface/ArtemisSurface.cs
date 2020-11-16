using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a surface of a specific scale, containing all the available <see cref="ArtemisDevice" />s
    /// </summary>
    public class ArtemisSurface : CorePropertyChanged
    {
        private List<ArtemisDevice> _devices;
        private bool _isActive;
        private string _name;
        private double _scale;

        internal ArtemisSurface(RGBSurface rgbSurface, string name, double scale)
        {
            SurfaceEntity = new SurfaceEntity {DeviceEntities = new List<DeviceEntity>()};
            EntityId = Guid.NewGuid();
            RgbSurface = rgbSurface;

            _name = name;
            _scale = scale;
            _isActive = false;

            // Devices are not populated here but as they are detected
            _devices = new List<ArtemisDevice>();

            ApplyToEntity();
        }

        internal ArtemisSurface(RGBSurface rgbSurface, SurfaceEntity surfaceEntity, double scale)
        {
            SurfaceEntity = surfaceEntity;
            EntityId = surfaceEntity.Id;
            RgbSurface = rgbSurface;

            _scale = scale;
            _name = surfaceEntity.Name;
            _isActive = surfaceEntity.IsActive;

            // Devices are not populated here but as they are detected
            _devices = new List<ArtemisDevice>();
        }

        /// <summary>
        ///     Gets the RGB.NET surface backing this Artemis surface
        /// </summary>
        public RGBSurface RgbSurface { get; }

        /// <summary>
        ///     Gets the scale at which this surface is rendered
        /// </summary>
        public double Scale
        {
            get => _scale;
            private set => SetAndNotify(ref _scale, value);
        }

        /// <summary>
        ///     Gets the name of the surface
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     Gets a boolean indicating whether this surface is the currently active surface
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            internal set => SetAndNotify(ref _isActive, value);
        }

        /// <summary>
        ///     Gets a list of devices this surface contains
        /// </summary>
        public List<ArtemisDevice> Devices
        {
            get => _devices;
            internal set => SetAndNotify(ref _devices, value);
        }

        internal SurfaceEntity SurfaceEntity { get; set; }
        internal Guid EntityId { get; set; }

        /// <summary>
        ///     Updates the scale of the surface
        /// </summary>
        /// <param name="value"></param>
        public void UpdateScale(double value)
        {
            Scale = value;
            foreach (ArtemisDevice device in Devices)
                device.CalculateRenderProperties();

            OnScaleChanged();
        }

        internal void ApplyToEntity()
        {
            SurfaceEntity.Id = EntityId;
            SurfaceEntity.Name = Name;
            SurfaceEntity.IsActive = IsActive;

            // Add missing device entities, don't remove old ones in case they come back later
            foreach (DeviceEntity deviceEntity in Devices.Select(d => d.DeviceEntity).ToList())
                if (!SurfaceEntity.DeviceEntities.Contains(deviceEntity))
                    SurfaceEntity.DeviceEntities.Add(deviceEntity);
        }

        #region Events

        /// <summary>
        ///     Occurs when the scale of the surface is changed
        /// </summary>
        public event EventHandler<EventArgs>? ScaleChanged;

        /// <summary>
        ///     Invokes the <see cref="ScaleChanged" /> event
        /// </summary>
        protected virtual void OnScaleChanged()
        {
            ScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}