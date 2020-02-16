using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.RGB.NET;
using RGB.NET.Core;

namespace Artemis.Core.Services.Interfaces
{
    public interface IRgbService : IArtemisService
    {
        /// <summary>
        ///     Gets or sets the RGB surface rendering is performed on
        /// </summary>
        RGBSurface Surface { get; set; }

        /// <summary>
        ///     Gets the bitmap brush used to convert the rendered frame to LED-colors
        /// </summary>
        BitmapBrush BitmapBrush { get; }

        /// <summary>
        ///     Gets the scale the frames are rendered on, a scale of 1.0 means 1 pixel = 1mm
        /// </summary>
        double RenderScale { get; }

        /// <summary>
        ///     Gets all loaded RGB devices
        /// </summary>
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

        void UpdateSurfaceLedGroup();
    }
}