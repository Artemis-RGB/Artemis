using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Services.Interfaces
{
    public interface IRgbService : IArtemisService
    {
        RGBSurface Surface { get; set; }
        GraphicsDecorator GraphicsDecorator { get; }
        IReadOnlyCollection<IRGBDevice> LoadedDevices { get; }

        void AddDeviceProvider(IRGBDeviceProvider deviceProvider);
        void Dispose();

        /// <summary>
        ///     Occurs when a single device has loaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceLoaded;

        /// <summary>
        ///     Occurs when a single device has reloaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceReloaded;
    }
}