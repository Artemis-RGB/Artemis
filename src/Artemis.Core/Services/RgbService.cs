using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.RGB.NET;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using Artemis.Storage.Entities;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Groups;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides wrapped access the RGB.NET
    /// </summary>
    public class RgbService : IRgbService, IDisposable
    {
        private readonly List<IRGBDevice> _loadedDevices;
        private readonly ILogger _logger;
        private readonly ISurfaceService _surfaceService;
        private readonly TimerUpdateTrigger _updateTrigger;

        internal RgbService(ILogger logger, ISurfaceService surfaceService)
        {
            _logger = logger;
            _surfaceService = surfaceService;
            Surface = RGBSurface.Instance;
            LoadingDevices = false;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;

            _loadedDevices = new List<IRGBDevice>();
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / 30};
            Surface.RegisterUpdateTrigger(_updateTrigger);
        }

        /// <inheritdoc />
        public bool LoadingDevices { get; }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public GraphicsDecorator GraphicsDecorator { get; private set; }

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            Surface.LoadDevices(deviceProvider);

            if (deviceProvider.Devices == null)
            {
                _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                return;
            }

            // Get the currently active surface configuration
            var surface = _surfaceService.GetActiveSurfaceConfiguration();
            if (surface == null)
                _logger.Information("No active surface configuration found, not positioning device");

            lock (_loadedDevices)
            {
                foreach (var surfaceDevice in deviceProvider.Devices)
                {
                    if (!_loadedDevices.Contains(surfaceDevice))
                    {
                        _loadedDevices.Add(surfaceDevice);
                        if (surface != null)
                            ApplyDeviceConfiguration(surfaceDevice, surface);
                        OnDeviceLoaded(new DeviceEventArgs(surfaceDevice));
                    }
                    else
                    {
                        if (surface != null)
                            ApplyDeviceConfiguration(surfaceDevice, surface);
                        OnDeviceReloaded(new DeviceEventArgs(surfaceDevice));
                    }
                }
            }

            // Apply the application wide brush and decorator
            var background = new ListLedGroup(Surface.Leds) {Brush = new SolidColorBrush(new Color(255, 255, 255, 255))};
            GraphicsDecorator = new GraphicsDecorator(background);
            background.Brush.AddDecorator(GraphicsDecorator);
        }

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(_updateTrigger);

            _updateTrigger.Dispose();
            Surface.Dispose();
        }

        public void ApplyDeviceConfiguration(IRGBDevice rgbDevice, SurfaceConfiguration surface)
        {
            // Determine the device ID by assuming devices are always added to the loaded devices list in the same order
            lock (_loadedDevices)
            {
                var deviceId = _loadedDevices.Where(d => d.DeviceInfo.DeviceName == rgbDevice.DeviceInfo.DeviceName &&
                                                         d.DeviceInfo.Model == rgbDevice.DeviceInfo.Model &&
                                                         d.DeviceInfo.Manufacturer == rgbDevice.DeviceInfo.Manufacturer)
                                   .ToList()
                                   .IndexOf(rgbDevice) + 1;

                var deviceConfig = surface.DeviceConfigurations.FirstOrDefault(d => d.DeviceName == rgbDevice.DeviceInfo.DeviceName &&
                                                                                       d.DeviceModel == rgbDevice.DeviceInfo.Model &&
                                                                                       d.DeviceManufacturer == rgbDevice.DeviceInfo.Manufacturer &&
                                                                                       d.DeviceId == deviceId);
                if (deviceConfig == null)
                {
                    _logger.Information("No surface device config found for {deviceInfo}, device ID: {deviceId}", rgbDevice.DeviceInfo, deviceId);
                    return;
                }

                rgbDevice.Location = new Point(deviceConfig.X, deviceConfig.Y);
            }
        }

        private void SurfaceOnException(ExceptionEventArgs args)
        {
            throw args.Exception;
        }

        #region Events

        public event EventHandler<DeviceEventArgs> DeviceLoaded;
        public event EventHandler<DeviceEventArgs> DeviceReloaded;

        private void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        private void OnDeviceReloaded(DeviceEventArgs e)
        {
            DeviceReloaded?.Invoke(this, e);
        }

        #endregion
    }
}