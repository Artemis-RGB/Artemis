using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Logitech;
using RGB.NET.Groups;

namespace Artemis.Core.Services
{
    public class RgbService : IRgbService, IDisposable
    {
        public RgbService()
        {
            Surface = RGBSurface.Instance;
            LoadingDevices = false;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
            Surface.UpdateMode = UpdateMode.Continuous;
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
                // Surface.LoadDevices(AsusDeviceProvider.Instance);
                Surface.LoadDevices(CorsairDeviceProvider.Instance);
                Surface.LoadDevices(LogitechDeviceProvider.Instance);
                Surface.LoadDevices(CoolerMasterDeviceProvider.Instance);
                // Surface.LoadDevices(NovationDeviceProvider.Instance);

                // TODO SpoinkyNL 8-1-18: Load alignment
                Surface.AlignDevies();

                // Do some testing, why does this work, how does it know I want to target the surface? Check source!
                var mouse1 = Surface.Leds.First(l => l.Id == LedId.Mouse1);
                mouse1.Color = new Color(255, 0, 0);
                var mouse2 = Surface.Leds.First(l => l.Id == LedId.Mouse2);
                mouse2.Color = new Color(255, 255, 0);
                var mouse3 = Surface.Leds.First(l => l.Id == LedId.Mouse3);
                mouse3.Color = new Color(255, 255, 255);
                var mouse4 = Surface.Leds.First(l => l.Id == LedId.Mouse4);
                mouse4.Color = new Color(255, 0, 255);
                Surface.UpdateMode = UpdateMode.Continuous;
            });

            OnFinishedLoadedDevices();
        }

        public void Dispose()
        {
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