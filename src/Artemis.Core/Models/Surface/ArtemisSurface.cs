using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<ArtemisDevice> _devices = new List<ArtemisDevice>();
        private ReadOnlyDictionary<Led, ArtemisLed> _ledMap = new ReadOnlyDictionary<Led, ArtemisLed>(new Dictionary<Led, ArtemisLed>());
        private bool _isActive;
        private string _name;

        internal ArtemisSurface(RGBSurface rgbSurface, string name)
        {
            SurfaceEntity = new SurfaceEntity {DeviceEntities = new List<DeviceEntity>()};
            EntityId = Guid.NewGuid();
            RgbSurface = rgbSurface;

            _name = name;
            _isActive = false;

            ApplyToEntity();
        }

        internal ArtemisSurface(RGBSurface rgbSurface, SurfaceEntity surfaceEntity)
        {
            SurfaceEntity = surfaceEntity;
            EntityId = surfaceEntity.Id;
            RgbSurface = rgbSurface;

            _name = surfaceEntity.Name;
            _isActive = surfaceEntity.IsActive;
        }

        /// <summary>
        ///     Gets the RGB.NET surface backing this Artemis surface
        /// </summary>
        public RGBSurface RgbSurface { get; }

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

        /// <summary>
        ///     Gets a dictionary containing all <see cref="ArtemisLed" />s on the surface with their corresponding RGB.NET
        ///     <see cref="Led" /> as key
        /// </summary>
        public ReadOnlyDictionary<Led, ArtemisLed> LedMap
        {
            get => _ledMap;
            private set => SetAndNotify(ref _ledMap, value);
        }

        internal SurfaceEntity SurfaceEntity { get; set; }
        internal Guid EntityId { get; set; }

        /// <summary>
        ///     Attempts to retrieve the <see cref="ArtemisLed" /> that corresponds the provided RGB.NET <see cref="Led" />
        /// </summary>
        /// <param name="led">The RGB.NET <see cref="Led" /> to find the corresponding <see cref="ArtemisLed" /> for </param>
        /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
        public ArtemisLed? GetArtemisLed(Led led)
        {
            LedMap.TryGetValue(led, out ArtemisLed? artemisLed);
            return artemisLed;
        }

        internal void UpdateLedMap()
        {
            LedMap = new ReadOnlyDictionary<Led, ArtemisLed>(
                _devices.SelectMany(d => d.Leds.Select(al => new KeyValuePair<Led, ArtemisLed>(al.RgbLed, al))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
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