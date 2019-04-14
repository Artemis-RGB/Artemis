using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.RGB.NET;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;
using RGB.NET.Groups;

namespace Artemis.Core.Services
{
    public class RgbService : IRgbService, IDisposable
    {
        private readonly List<IRGBDevice> _loadedDevices;
        private readonly TimerUpdateTrigger _updateTrigger;

        public RgbService()
        {
            Surface = RGBSurface.Instance;
            LoadingDevices = false;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;

            _loadedDevices = new List<IRGBDevice>();
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / 30};
            Surface.RegisterUpdateTrigger(_updateTrigger);
        }

        /// <inheritdoc />
        public bool LoadingDevices { get; private set; }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public GraphicsDecorator GraphicsDecorator { get; private set; }

        /// <inheritdoc />
        public async Task LoadDevices()
        {
            OnStartedLoadingDevices();

            await Task.Run(() =>
            {
                // TODO SpoinkyNL 8-1-18: Keep settings into account
                // This one doesn't work well without ASUS devices installed
                // Surface.LoadDevices(AsusDeviceProvider.Instance);
                // Surface.LoadDevices(CoolerMasterDeviceProvider.Instance);
                Surface.LoadDevices(CorsairDeviceProvider.Instance);
                // Surface.LoadDevices(DMXDeviceProvider.Instance);
                // Surface.LoadDevices(LogitechDeviceProvider.Instance);
                // Surface.LoadDevices(MsiDeviceProvider.Instance);
                // Surface.LoadDevices(NovationDeviceProvider.Instance);
                // Surface.LoadDevices(RazerDeviceProvider.Instance);

                // TODO SpoinkyNL 8-1-18: Load alignment
                Surface.AlignDevices();

                lock (_loadedDevices)
                {
                    foreach (var surfaceDevice in Surface.Devices)
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
            });

            // Apply the application wide brush and decorator
            var background = new ListLedGroup(Surface.Leds) {Brush = new SolidColorBrush(new Color(255, 255, 255, 255))};
            GraphicsDecorator = new GraphicsDecorator(background);
            background.Brush.AddDecorator(GraphicsDecorator);

            OnFinishedLoadedDevices();
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
        public event EventHandler StartingLoadingDevices;
        public event EventHandler FinishedLoadedDevices;

        private void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        private void OnDeviceReloaded(DeviceEventArgs e)
        {
            DeviceReloaded?.Invoke(this, e);
        }

        private void OnStartedLoadingDevices()
        {
            LoadingDevices = true;
            StartingLoadingDevices?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinishedLoadedDevices()
        {
            LoadingDevices = false;
            FinishedLoadedDevices?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}