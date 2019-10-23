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
        private SurfaceConfiguration _activeSurfaceConfiguration;

        public SurfaceService(ILogger logger, ISurfaceRepository surfaceRepository, IRgbService rgbService)
        {
            _logger = logger;
            _surfaceRepository = surfaceRepository;
            _rgbService = rgbService;
            _surfaceConfigurations = new List<SurfaceConfiguration>();

            LoadFromRepository();

            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
        }

        public SurfaceConfiguration ActiveSurfaceConfiguration
        {
            get => _activeSurfaceConfiguration;
            set
            {
                if (_activeSurfaceConfiguration == value)
                    return;

                _activeSurfaceConfiguration = value;
                lock (_surfaceConfigurations)
                {
                    // Mark only the new value as active
                    foreach (var surfaceConfiguration in _surfaceConfigurations)
                        surfaceConfiguration.IsActive = false;
                    _activeSurfaceConfiguration.IsActive = true;

                    SaveToRepository(_surfaceConfigurations, true);
                }

                // Apply the active surface configuration to the devices
                if (ActiveSurfaceConfiguration != null)
                {
                    foreach (var deviceConfiguration in ActiveSurfaceConfiguration.DeviceConfigurations)
                        deviceConfiguration.ApplyToDevice();
                }
                // Update the RGB service's graphics decorator to work with the new surface configuration
                _rgbService.UpdateGraphicsDecorator();

                OnActiveSurfaceConfigurationChanged(new SurfaceConfigurationEventArgs(_activeSurfaceConfiguration));
            }
        }

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
                SaveToRepository(configuration, true);
                return configuration;
            }
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

            foreach (var deviceConfiguration in ActiveSurfaceConfiguration.DeviceConfigurations) 
                deviceConfiguration.ApplyToDevice();
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
                ActiveSurfaceConfiguration = active;
        }

        public void SaveToRepository(List<SurfaceConfiguration> surfaceConfigurations, bool includeDevices)
        {
            foreach (var surfaceConfiguration in surfaceConfigurations)
            {
                surfaceConfiguration.ApplyToEntity();
                if (!includeDevices)
                {
                    foreach (var deviceConfiguration in surfaceConfiguration.DeviceConfigurations)
                        deviceConfiguration.ApplyToEntity();
                }
            }

            _surfaceRepository.Save();
        }

        public void SaveToRepository(SurfaceConfiguration surfaceConfiguration, bool includeDevices)
        {
            surfaceConfiguration.ApplyToEntity();
            if (includeDevices)
            {
                foreach (var deviceConfiguration in surfaceConfiguration.DeviceConfigurations)
                    deviceConfiguration.ApplyToEntity();
            }

            _surfaceRepository.Save();
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

        protected virtual void OnActiveSurfaceConfigurationChanged(SurfaceConfigurationEventArgs e)
        {
            ActiveSurfaceConfigurationChanged?.Invoke(this, e);
        }

        #endregion
    }
}