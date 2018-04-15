using System;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Services.Interfaces
{
    public interface IRgbService : IArtemisService
    {
        bool LoadingDevices { get; }
        RGBSurface Surface { get; set; }
        GraphicsDecorator GraphicsDecorator { get; }
        Task LoadDevices();
        void Dispose();

        /// <summary>
        ///     Occurs when a single device has loaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceLoaded;

        /// <summary>
        ///     Occurs when a single device has reloaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceReloaded;

        /// <summary>
        ///     Occurs when loading all devices has started
        /// </summary>
        event EventHandler StartingLoadingDevices;

        /// <summary>
        ///     Occurs when loading all devices has finished
        /// </summary>
        event EventHandler FinishedLoadedDevices;
    }
}