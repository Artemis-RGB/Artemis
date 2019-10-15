using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
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
        private readonly ISurfaceRepository _surfaceRepository;
        private readonly IRgbService _rgbService;

        public SurfaceService(ILogger logger, ISurfaceRepository surfaceRepository, IRgbService rgbService)
        {
            _logger = logger;
            _surfaceRepository = surfaceRepository;
            _rgbService = rgbService;

            _rgbService.DeviceLoaded += RgbServiceOnDeviceLoaded;
        }

        public async Task<List<SurfaceConfiguration>> GetSurfaceConfigurationsAsync()
        {
            var surfaceEntities = await _surfaceRepository.GetAllAsync();
            var configs = new List<SurfaceConfiguration>();
            foreach (var surfaceEntity in surfaceEntities)
                configs.Add(new SurfaceConfiguration(surfaceEntity));

            return configs;
        }

        public async Task<SurfaceConfiguration> GetActiveSurfaceConfigurationAsync()
        {
            var entity = (await _surfaceRepository.GetAllAsync()).FirstOrDefault(d => d.IsActive);
            return entity != null ? new SurfaceConfiguration(entity) : null;
        }

        public async Task SetActiveSurfaceConfigurationAsync(SurfaceConfiguration surfaceConfiguration)
        {
            var surfaceEntities = await _surfaceRepository.GetAllAsync();
            foreach (var surfaceEntity in surfaceEntities)
                surfaceEntity.IsActive = surfaceEntity.Guid == surfaceConfiguration.Guid;

            await _surfaceRepository.SaveAsync();
        }

        public List<SurfaceConfiguration> GetSurfaceConfigurations()
        {
            var surfaceEntities = _surfaceRepository.GetAll();
            var configs = new List<SurfaceConfiguration>();
            foreach (var surfaceEntity in surfaceEntities)
                configs.Add(new SurfaceConfiguration(surfaceEntity));

            return configs;
        }

        public SurfaceConfiguration GetActiveSurfaceConfiguration()
        {
            var entity = _surfaceRepository.GetAll().FirstOrDefault(d => d.IsActive);
            return entity != null ? new SurfaceConfiguration(entity) : null;
        }

        public void SetActiveSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration)
        {
            var surfaceEntities = _surfaceRepository.GetAll();
            foreach (var surfaceEntity in surfaceEntities)
                surfaceEntity.IsActive = surfaceEntity.Guid == surfaceConfiguration.Guid;

            _surfaceRepository.Save();
        }

        public SurfaceConfiguration CreateSurfaceConfiguration(string name)
        {
            // Create a blank config
            var configuration = new SurfaceConfiguration {Name = name, DeviceConfigurations = new List<SurfaceDeviceConfiguration>()};

            // Add all current devices
            foreach (var rgbDevice in _rgbService.LoadedDevices)
            {
                var deviceId = GetDeviceId(rgbDevice);
                configuration.DeviceConfigurations.Add(new SurfaceDeviceConfiguration(deviceId, rgbDevice.DeviceInfo, configuration));
            }

            return configuration;
        }

        private void ApplyDeviceConfiguration(IRGBDevice rgbDevice, SurfaceConfiguration surface)
        {
            var deviceId = GetDeviceId(rgbDevice);
            var deviceConfig = surface.DeviceConfigurations.FirstOrDefault(d => d.DeviceName == rgbDevice.DeviceInfo.DeviceName &&
                                                                                d.DeviceModel == rgbDevice.DeviceInfo.Model &&
                                                                                d.DeviceManufacturer == rgbDevice.DeviceInfo.Manufacturer &&
                                                                                d.DeviceId == deviceId);

            if (deviceConfig == null)
            {
                _logger.Information("No active surface config found for {deviceInfo}, device ID: {deviceId}. Adding a new entry.", rgbDevice.DeviceInfo, deviceId);
                deviceConfig = new SurfaceDeviceConfiguration(deviceId, rgbDevice.DeviceInfo, surface);
                surface.DeviceConfigurations.Add(deviceConfig);
            }

            rgbDevice.Location = new Point(deviceConfig.X, deviceConfig.Y);
            OnDeviceConfigurationApplied(new SurfaceConfigurationEventArgs(surface, rgbDevice));
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

        private void RgbServiceOnDeviceLoaded(object sender, DeviceEventArgs e)
        {
            var activeConfiguration = GetActiveSurfaceConfiguration();
            if (activeConfiguration == null)
            {
                _logger.Information("No active surface config found, cannot apply settings to {deviceInfo}", e.Device.DeviceInfo);
                return;
            }

            ApplyDeviceConfiguration(e.Device, GetActiveSurfaceConfiguration());
        }

        #region Events

        public event EventHandler<SurfaceConfigurationEventArgs> SurfaceConfigurationApplied;

        private void OnDeviceConfigurationApplied(SurfaceConfigurationEventArgs e)
        {
            SurfaceConfigurationApplied?.Invoke(this, e);
        }

        #endregion
    }

    public interface ISurfaceService : IArtemisService
    {
        Task<List<SurfaceConfiguration>> GetSurfaceConfigurationsAsync();
        Task<SurfaceConfiguration> GetActiveSurfaceConfigurationAsync();
        Task SetActiveSurfaceConfigurationAsync(SurfaceConfiguration surfaceConfiguration);
        List<SurfaceConfiguration> GetSurfaceConfigurations();
        SurfaceConfiguration GetActiveSurfaceConfiguration();
        SurfaceConfiguration CreateSurfaceConfiguration(string name);
        void SetActiveSurfaceConfiguration(SurfaceConfiguration surfaceConfiguration);

        /// <summary>
        ///     Occurs when a device has a new surface configuration applied to it
        /// </summary>
        event EventHandler<SurfaceConfigurationEventArgs> SurfaceConfigurationApplied;
    }
}