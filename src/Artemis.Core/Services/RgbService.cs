using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Devices.Asus;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.DMX;
using RGB.NET.Devices.Logitech;
using RGB.NET.Devices.Msi;
using RGB.NET.Devices.Novation;
using RGB.NET.Devices.Razer;
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
                Surface.LoadDevices(AsusDeviceProvider.Instance);
                Surface.LoadDevices(CoolerMasterDeviceProvider.Instance);
                Surface.LoadDevices(CorsairDeviceProvider.Instance);
                Surface.LoadDevices(DMXDeviceProvider.Instance);
                Surface.LoadDevices(LogitechDeviceProvider.Instance);
                Surface.LoadDevices(NovationDeviceProvider.Instance);
                Surface.LoadDevices(MsiDeviceProvider.Instance);
                Surface.LoadDevices(RazerDeviceProvider.Instance);

                // TODO SpoinkyNL 8-1-18: Load alignment
                Surface.AlignDevices();

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