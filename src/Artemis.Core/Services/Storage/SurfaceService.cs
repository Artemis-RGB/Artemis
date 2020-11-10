using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services
{
    internal class SurfaceService : ISurfaceService
    {
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly PluginSetting<double> _renderScaleSetting;
        private readonly IRgbService _rgbService;
        private readonly List<ArtemisSurface> _surfaceConfigurations;
        private readonly ISurfaceRepository _surfaceRepository;

        public SurfaceService(ILogger logger, ISurfaceRepository surfaceRepository, IRgbService rgbService, IPluginManagementService pluginManagementService, ISettingsService settingsService)
        {
            _logger = logger;
            _surfaceRepository = surfaceRepository;
            _rgbService = rgbService;
            _pluginManagementService = pluginManagementService;
            _surfaceConfigurations = new List<ArtemisSurface>();
            _renderScaleSetting = settingsService.GetSetting("Core.RenderScale", 0.5);

            LoadFromRepository();

            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
        }

        public ArtemisSurface ActiveSurface { get; private set; }
        public ReadOnlyCollection<ArtemisSurface> SurfaceConfigurations => _surfaceConfigurations.AsReadOnly();

        public ArtemisSurface CreateSurfaceConfiguration(string name)
        {
            // Create a blank config
            ArtemisSurface configuration = new ArtemisSurface(_rgbService.Surface, name, _renderScaleSetting.Value);

            // Add all current devices
            foreach (IRGBDevice rgbDevice in _rgbService.LoadedDevices)
            {
                PluginImplementation pluginImplementation = _pluginManagementService.GetPluginByDevice(rgbDevice);
                configuration.Devices.Add(new ArtemisDevice(rgbDevice, pluginImplementation, configuration));
            }

            lock (_surfaceConfigurations)
            {
                _surfaceRepository.Add(configuration.SurfaceEntity);
                _surfaceConfigurations.Add(configuration);

                UpdateSurfaceConfiguration(configuration, true);
                return configuration;
            }
        }

        public void SetActiveSurfaceConfiguration(ArtemisSurface surface)
        {
            if (ActiveSurface == surface)
                return;

            // Set the new entity
            ActiveSurface = surface;

            // Ensure only the new entity is marked as active
            lock (_surfaceConfigurations)
            {
                // Mark only the new surface as active
                foreach (ArtemisSurface configuration in _surfaceConfigurations)
                {
                    configuration.IsActive = configuration == ActiveSurface;
                    configuration.ApplyToEntity();

                    _surfaceRepository.Save(configuration.SurfaceEntity);
                }
            }

            // Apply the active surface entity to the devices
            if (ActiveSurface != null)
            {
                foreach (ArtemisDevice device in ActiveSurface.Devices)
                    device.ApplyToRgbDevice();
            }

            // Update the RGB service's graphics decorator to work with the new surface entity
            _rgbService.UpdateSurfaceLedGroup();
            OnActiveSurfaceConfigurationChanged(new SurfaceConfigurationEventArgs(ActiveSurface));
        }

        public void UpdateSurfaceConfiguration(ArtemisSurface surface, bool includeDevices)
        {
            surface.ApplyToEntity();
            if (includeDevices)
            {
                foreach (ArtemisDevice deviceConfiguration in surface.Devices)
                {
                    deviceConfiguration.ApplyToEntity();
                    if (surface.IsActive)
                        deviceConfiguration.ApplyToRgbDevice();
                }
            }

            _surfaceRepository.Save(surface.SurfaceEntity);
            _rgbService.UpdateSurfaceLedGroup();
            OnSurfaceConfigurationUpdated(new SurfaceConfigurationEventArgs(surface));
        }

        public void DeleteSurfaceConfiguration(ArtemisSurface surface)
        {
            if (surface == ActiveSurface)
                throw new ArtemisCoreException($"Cannot delete surface entity '{surface.Name}' because it is active.");

            lock (_surfaceConfigurations)
            {
                SurfaceEntity entity = surface.SurfaceEntity;
                _surfaceConfigurations.Remove(surface);
                _surfaceRepository.Remove(entity);
            }
        }

        #region Repository

        private void LoadFromRepository()
        {
            List<SurfaceEntity> configs = _surfaceRepository.GetAll();
            foreach (SurfaceEntity surfaceEntity in configs)
            {
                // Create the surface entity
                ArtemisSurface surfaceConfiguration = new ArtemisSurface(_rgbService.Surface, surfaceEntity, _renderScaleSetting.Value);
                foreach (DeviceEntity position in surfaceEntity.DeviceEntities)
                {
                    IRGBDevice device = _rgbService.Surface.Devices.FirstOrDefault(d => d.GetDeviceIdentifier() == position.DeviceIdentifier);
                    if (device != null)
                    {
                        PluginImplementation pluginImplementation = _pluginManagementService.GetPluginByDevice(device);
                        surfaceConfiguration.Devices.Add(new ArtemisDevice(device, pluginImplementation, surfaceConfiguration, position));
                    }
                }

                // Finally, add the surface config to the collection
                lock (_surfaceConfigurations)
                {
                    _surfaceConfigurations.Add(surfaceConfiguration);
                }
            }

            // When all surface configs are loaded, apply the active surface config
            ArtemisSurface active = SurfaceConfigurations.FirstOrDefault(c => c.IsActive);
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

        private void AddDeviceIfMissing(IRGBDevice rgbDevice, ArtemisSurface surface)
        {
            string deviceIdentifier = rgbDevice.GetDeviceIdentifier();
            ArtemisDevice device = surface.Devices.FirstOrDefault(d => d.DeviceEntity.DeviceIdentifier == deviceIdentifier);

            if (device != null)
                return;

            // Find an existing device config and use that
            DeviceEntity existingDeviceConfig = surface.SurfaceEntity.DeviceEntities.FirstOrDefault(d => d.DeviceIdentifier == deviceIdentifier);
            if (existingDeviceConfig != null)
            {
                PluginImplementation pluginImplementation = _pluginManagementService.GetPluginByDevice(rgbDevice);
                device = new ArtemisDevice(rgbDevice, pluginImplementation, surface, existingDeviceConfig);
            }
            // Fall back on creating a new device
            else
            {
                _logger.Information(
                    "No device config found for {deviceInfo}, device hash: {deviceHashCode}. Adding a new entry.",
                    rgbDevice.DeviceInfo,
                    deviceIdentifier
                );
                PluginImplementation pluginImplementation = _pluginManagementService.GetPluginByDevice(rgbDevice);
                device = new ArtemisDevice(rgbDevice, pluginImplementation, surface);
            }

            surface.Devices.Add(device);
        }

        #endregion

        #region Event handlers

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            lock (_surfaceConfigurations)
            {
                foreach (ArtemisSurface surfaceConfiguration in _surfaceConfigurations)
                    AddDeviceIfMissing(e.Device, surfaceConfiguration);
            }

            UpdateSurfaceConfiguration(ActiveSurface, true);
        }

        private void RenderScaleSettingOnSettingChanged(object sender, EventArgs e)
        {
            foreach (ArtemisSurface surfaceConfiguration in SurfaceConfigurations)
            {
                surfaceConfiguration.UpdateScale(_renderScaleSetting.Value);
                OnSurfaceConfigurationUpdated(new SurfaceConfigurationEventArgs(surfaceConfiguration));
            }
        }

        #endregion

        #region Events

        public event EventHandler<SurfaceConfigurationEventArgs> ActiveSurfaceConfigurationSelected;
        public event EventHandler<SurfaceConfigurationEventArgs> SurfaceConfigurationUpdated;

        protected virtual void OnActiveSurfaceConfigurationChanged(SurfaceConfigurationEventArgs e)
        {
            ActiveSurfaceConfigurationSelected?.Invoke(this, e);
        }

        protected virtual void OnSurfaceConfigurationUpdated(SurfaceConfigurationEventArgs e)
        {
            SurfaceConfigurationUpdated?.Invoke(this, e);
        }

        #endregion
    }
}