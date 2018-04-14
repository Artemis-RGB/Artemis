using System;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;

namespace Artemis.Core.Services
{
    public class RgbService : IRgbService, IDisposable
    {
        private readonly TimerUpdateTrigger _updateTrigger;

        public RgbService()
        {
            Surface = RGBSurface.Instance;
            LoadingDevices = false;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;

            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / 30};
            Surface.RegisterUpdateTrigger(_updateTrigger);
        }

        /// <inheritdoc />
        public bool LoadingDevices { get; private set; }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

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
            });

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