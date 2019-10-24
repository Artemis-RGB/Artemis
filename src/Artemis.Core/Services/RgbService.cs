using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.RGB.NET;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
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
        private readonly TimerUpdateTrigger _updateTrigger;
        private ListLedGroup _background;

        internal RgbService(ILogger logger)
        {
            _logger = logger;
            Surface = RGBSurface.Instance;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;

            _loadedDevices = new List<IRGBDevice>();
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / 25};
            Surface.RegisterUpdateTrigger(_updateTrigger);
        }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public GraphicsDecorator GraphicsDecorator { get; private set; }

        public IReadOnlyCollection<IRGBDevice> LoadedDevices
        {
            get
            {
                lock (_loadedDevices)
                {
                    return _loadedDevices.AsReadOnly();
                }
            }
        }

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            Surface.LoadDevices(deviceProvider);

            if (deviceProvider.Devices == null)
            {
                _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                return;
            }

            lock (_loadedDevices)
            {
                foreach (var surfaceDevice in deviceProvider.Devices)
                {
                    if (!_loadedDevices.Contains(surfaceDevice))
                    {
                        _loadedDevices.Add(surfaceDevice);
                        OnDeviceLoaded(new DeviceEventArgs(surfaceDevice));
                    }
                    else
                    {
                        OnDeviceReloaded(new DeviceEventArgs(surfaceDevice));
                    }
                }
            }
        }

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(_updateTrigger);

            _updateTrigger.Dispose();
            Surface.Dispose();
        }

        private void SurfaceOnException(ExceptionEventArgs args)
        {
            throw args.Exception;
        }

        #region Events

        public event EventHandler<DeviceEventArgs> DeviceLoaded;
        public event EventHandler<DeviceEventArgs> DeviceReloaded;

        public void UpdateGraphicsDecorator()
        {
            // TODO: Create new one first, then clean up the old one for a smoother transition

            // Clean up the old background if present
            if (_background != null)
            {
                _background.Brush?.RemoveAllDecorators();
                _background.Detach();
            }

            // Apply the application wide brush and decorator
            _background = new ListLedGroup(Surface.Leds) {Brush = new SolidColorBrush(new Color(255, 255, 255, 255))};
            GraphicsDecorator = new GraphicsDecorator(_background);
            _background.Brush.RemoveAllDecorators();

            _background.Brush.AddDecorator(GraphicsDecorator);
        }

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