using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services.Storage
{
    public class SurfaceService : ISurfaceService
    {
        private readonly ILogger _logger;
        private readonly IRgbService _rgbService;
        private readonly List<Surface> _surfaceConfigurations;
        private readonly ISurfaceRepository _surfaceRepository;
        private readonly PluginSetting<double> _renderScaleSetting;

        internal SurfaceService(ILogger logger, ISurfaceRepository surfaceRepository, IRgbService rgbService, ISettingsService settingsService)
        {
            _logger = logger;
            _surfaceRepository = surfaceRepository;
            _rgbService = rgbService;
            _surfaceConfigurations = new List<Surface>();
            _renderScaleSetting = settingsService.GetSetting("RenderScale", 1.0);

            LoadFromRepository();

            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
        }



        public Surface ActiveSurface { get; private set; }
        public ReadOnlyCollection<Surface> SurfaceConfigurations => _surfaceConfigurations.AsReadOnly();

        public Surface CreateSurfaceConfiguration(string name)
        {
            // Create a blank config
            var configuration = new Surface(_rgbService.Surface, name, _renderScaleSetting.Value);

            // Add all current devices
            foreach (var rgbDevice in _rgbService.LoadedDevices) 
                configuration.Devices.Add(new Device(rgbDevice, configuration));

            lock (_surfaceConfigurations)
            {
                _surfaceRepository.Add(configuration.SurfaceEntity);
                UpdateSurfaceConfiguration(configuration, true);
                return configuration;
            }
        }

        public void SetActiveSurfaceConfiguration(Surface surface)
        {
            if (ActiveSurface == surface)
                return;

            // Set the new entity
            ActiveSurface = surface;

            // Ensure only the new entity is marked as active
            lock (_surfaceConfigurations)
            {
                // Mark only the new surface as active
                foreach (var configuration in _surfaceConfigurations)
                {
                    configuration.IsActive = configuration == ActiveSurface;
                    configuration.ApplyToEntity();
                }

                _surfaceRepository.Save();
            }

            // Apply the active surface entity to the devices
            if (ActiveSurface != null)
            {
                foreach (var device in ActiveSurface.Devices)
                    device.ApplyToRgbDevice();
            }

            // Update the RGB service's graphics decorator to work with the new surface entity
            _rgbService.UpdateGraphicsDecorator();
            OnActiveSurfaceConfigurationChanged(new SurfaceConfigurationEventArgs(ActiveSurface));
        }

        public void UpdateSurfaceConfiguration(Surface surface, bool includeDevices)
        {
            surface.ApplyToEntity();
            if (includeDevices)
            {
                foreach (var deviceConfiguration in surface.Devices)
                {
                    deviceConfiguration.ApplyToConfiguration();
                    if (surface.IsActive)
                        deviceConfiguration.ApplyToRgbDevice();
                }
            }

            _surfaceRepository.Save();
            _rgbService.UpdateGraphicsDecorator();
            OnSurfaceConfigurationUpdated(new SurfaceConfigurationEventArgs(surface));
        }

        public void DeleteSurfaceConfiguration(Surface surface)
        {
            if (surface == ActiveSurface)
                throw new ArtemisCoreException($"Cannot delete surface entity '{surface.Name}' because it is active.");

            lock (_surfaceConfigurations)
            {
                var entity = surface.SurfaceEntity;
                surface.Destroy();
                _surfaceConfigurations.Remove(surface);

                _surfaceRepository.Remove(entity);
                _surfaceRepository.Save();
            }
        }

        #region Event handlers

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            lock (_surfaceConfigurations)
            {
                foreach (var surfaceConfiguration in _surfaceConfigurations)
                    AddDeviceIfMissing(e.Device, surfaceConfiguration);
            }

            UpdateSurfaceConfiguration(ActiveSurface, true);
        }

        private void RenderScaleSettingOnSettingChanged(object sender, EventArgs e)
        {
            foreach (var surfaceConfiguration in SurfaceConfigurations) 
                surfaceConfiguration.UpdateScale(_renderScaleSetting.Value);

        }

        #endregion

        #region Repository

        private void LoadFromRepository()
        {
            var configs = _surfaceRepository.GetAll();
            foreach (var surfaceEntity in configs)
            {
                // Create the surface entity
                var surfaceConfiguration = new Surface(_rgbService.Surface, surfaceEntity, _renderScaleSetting.Value);

                // Finally, add the surface config to the collection
                lock (_surfaceConfigurations)
                {
                    _surfaceConfigurations.Add(surfaceConfiguration);
                }
            }

            // When all surface configs are loaded, apply the active surface config
            var active = SurfaceConfigurations.FirstOrDefault(c => c.IsActive);
            if (active != null)
                SetActiveSurfaceConfiguration(active);
            else
            {
                active = SurfaceConfigurations.FirstOrDefault();
                if (active != null)
                    SetActiveSurfaceConfiguration(active);
                else
                    SetActiveSurfaceConfiguration(CreateSurfaceConfiguration("Default"));
            }
        }

        #endregion

        #region Utilities

        private void AddDeviceIfMissing(IRGBDevice rgbDevice, Surface surface)
        {
            var deviceHashCode = rgbDevice.GetDeviceHashCode();
            var device = surface.Devices.FirstOrDefault(d => d.Configuration.DeviceHashCode == deviceHashCode);

            if (device != null)
                return;
            
            // Find an existing device config and use that
            var existingDeviceConfig = surface.SurfaceEntity.DeviceEntities.FirstOrDefault(d => d.DeviceHashCode == deviceHashCode);
            if (existingDeviceConfig != null) 
                device = new Device(rgbDevice,surface, existingDeviceConfig);
            // Fall back on creating a new device
            else
            {
                _logger.Information(
                    "No device config found for {deviceInfo}, device hash: {deviceHashCode}. Adding a new entry.",
                    rgbDevice.DeviceInfo,
                    deviceHashCode
                );
                device = new Device(rgbDevice, surface);
            }

            surface.Devices.Add(device);
        }

        #endregion

        #region Events

        public event EventHandler<SurfaceConfigurationEventArgs> ActiveSurfaceConfigurationChanged;
        public event EventHandler<SurfaceConfigurationEventArgs> SurfaceConfigurationUpdated;

        protected virtual void OnActiveSurfaceConfigurationChanged(SurfaceConfigurationEventArgs e)
        {
            ActiveSurfaceConfigurationChanged?.Invoke(this, e);
        }

        protected virtual void OnSurfaceConfigurationUpdated(SurfaceConfigurationEventArgs e)
        {
            SurfaceConfigurationUpdated?.Invoke(this, e);
        }

        #endregion
    }
}