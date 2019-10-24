using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Surface;
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
        private readonly List<SurfaceConfiguration> _surfaceConfigurations;
        private readonly ISurfaceRepository _surfaceRepository;

        public SurfaceService(ILogger logger, ISurfaceRepository surfaceRepository, IRgbService rgbService)
        {
            _logger = logger;
            _surfaceRepository = surfaceRepository;
            _rgbService = rgbService;
            _surfaceConfigurations = new List<SurfaceConfiguration>();

            LoadFromRepository();

            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
        }

        public SurfaceConfiguration ActiveSurfaceConfiguration { get; private set; }

        public ReadOnlyCollection<SurfaceConfiguration> SurfaceConfigurations
        {
            get
            {
                lock (_surfaceConfigurations)
                {
                    return _surfaceConfigurations.AsReadOnly();
                }
            }
        }

        public SurfaceConfiguration CreateSurfaceConfiguration(string name)
        {
            // Create a blank config
            var configuration = new SurfaceConfiguration(name);

            // Add all current devices
            foreach (var rgbDevice in _rgbService.LoadedDevices)
            {
                var deviceId = GetDeviceId(rgbDevice);
                configuration.DeviceConfigurations.Add(new SurfaceDeviceConfiguration(rgbDevice, deviceId, configuration));
            }

            lock (_surfaceConfigurations)
            {
                _surfaceRepository.Add(configuration.SurfaceEntity);
                UpdateSurfaceConfiguration(configuration, true);
                return configuration;
            }
        }

        public void SetActiveSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration)
        {
            if (ActiveSurfaceConfiguration == surfaceConfiguration)
                return;

            // Set the new configuration
            ActiveSurfaceConfiguration = surfaceConfiguration;

            // Ensure only the new configuration is marked as active
            lock (_surfaceConfigurations)
            {
                // Mark only the new surfaceConfiguration as active
                foreach (var configuration in _surfaceConfigurations)
                {
                    configuration.IsActive = configuration == ActiveSurfaceConfiguration;
                    configuration.ApplyToEntity();
                }

                _surfaceRepository.Save();
            }

            // Apply the active surface configuration to the devices
            if (ActiveSurfaceConfiguration != null)
            {
                foreach (var deviceConfiguration in ActiveSurfaceConfiguration.DeviceConfigurations)
                    deviceConfiguration.ApplyToDevice();
            }

            // Update the RGB service's graphics decorator to work with the new surface configuration
            _rgbService.UpdateGraphicsDecorator();
            OnActiveSurfaceConfigurationChanged(new SurfaceConfigurationEventArgs(ActiveSurfaceConfiguration));
        }

        public void UpdateSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration, bool includeDevices)
        {
            surfaceConfiguration.ApplyToEntity();
            if (includeDevices)
            {
                foreach (var deviceConfiguration in surfaceConfiguration.DeviceConfigurations)
                {
                    deviceConfiguration.ApplyToEntity();
                    if (surfaceConfiguration.IsActive)
                        deviceConfiguration.ApplyToDevice();
                }
            }

            _surfaceRepository.Save();
            _rgbService.UpdateGraphicsDecorator();
            OnSurfaceConfigurationUpdated(new SurfaceConfigurationEventArgs(surfaceConfiguration));
        }

        public void DeleteSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration)
        {
            if (surfaceConfiguration == ActiveSurfaceConfiguration)
                throw new ArtemisCoreException($"Cannot delete surface configuration '{surfaceConfiguration.Name}' because it is active.");

            lock (_surfaceConfigurations)
            {
                surfaceConfiguration.Destroy();
                _surfaceConfigurations.Remove(surfaceConfiguration);

                _surfaceRepository.Remove(surfaceConfiguration.SurfaceEntity);
                _surfaceRepository.Save();
            }
        }

        #region Event handlers

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            lock (_surfaceConfigurations)
            {
                foreach (var surfaceConfiguration in _surfaceConfigurations)
                    MatchDeviceConfiguration(e.Device, surfaceConfiguration);
            }

            UpdateSurfaceConfiguration(ActiveSurfaceConfiguration, true);
        }

        #endregion

        #region Repository

        private void LoadFromRepository()
        {
            var configs = _surfaceRepository.GetAll();
            foreach (var surfaceEntity in configs)
            {
                // Create the surface configuration
                var surfaceConfiguration = new SurfaceConfiguration(surfaceEntity);
                // For each loaded device, match a device configuration
                var devices = _rgbService.LoadedDevices;
                foreach (var rgbDevice in devices)
                    MatchDeviceConfiguration(rgbDevice, surfaceConfiguration);
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
        }

        #endregion

        #region Utilities

        private void MatchDeviceConfiguration(IRGBDevice rgbDevice, SurfaceConfiguration surfaceConfiguration)
        {
            var deviceId = GetDeviceId(rgbDevice);
            var deviceConfig = surfaceConfiguration.DeviceConfigurations.FirstOrDefault(d => d.DeviceName == rgbDevice.DeviceInfo.DeviceName &&
                                                                                             d.DeviceModel == rgbDevice.DeviceInfo.Model &&
                                                                                             d.DeviceManufacturer == rgbDevice.DeviceInfo.Manufacturer &&
                                                                                             d.DeviceId == deviceId);

            if (deviceConfig == null)
            {
                _logger.Information("No active surface config found for {deviceInfo}, device ID: {deviceId}. Adding a new entry.", rgbDevice.DeviceInfo, deviceId);
                deviceConfig = new SurfaceDeviceConfiguration(rgbDevice, deviceId, surfaceConfiguration);
                surfaceConfiguration.DeviceConfigurations.Add(deviceConfig);
            }

            deviceConfig.Device = rgbDevice;
            deviceConfig.ApplyToDevice();
        }

        private int GetDeviceId(IRGBDevice rgbDevice)
        {
            return _rgbService.LoadedDevices
                       .Where(d => d.DeviceInfo.DeviceName == rgbDevice.DeviceInfo.DeviceName &&
                                   d.DeviceInfo.Model == rgbDevice.DeviceInfo.Model &&
                                   d.DeviceInfo.Manufacturer == rgbDevice.DeviceInfo.Manufacturer)
                       .ToList()
                       .IndexOf(rgbDevice) + 1;
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