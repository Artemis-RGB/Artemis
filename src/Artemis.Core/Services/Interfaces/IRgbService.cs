using System;
using System.Collections.Generic;
using RGB.NET.Core;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to manage the <see cref="RGBSurface" /> and its contents
    /// </summary>
    public interface IRgbService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Gets or sets the RGB surface rendering is performed on
        /// </summary>
        RGBSurface Surface { get; set; }

        /// <summary>
        ///     Gets the bitmap brush used to convert the rendered frame to LED-colors
        /// </summary>
        BitmapBrush? BitmapBrush { get; }

        /// <summary>
        ///     Gets the scale the frames are rendered on, a scale of 1.0 means 1 pixel = 1mm
        /// </summary>
        double RenderScale { get; }

        /// <summary>
        ///     Gets all loaded RGB devices
        /// </summary>
        IReadOnlyCollection<IRGBDevice> LoadedDevices { get; }

        /// <summary>
        ///     Gets the update trigger that drives the render loop
        /// </summary>
        TimerUpdateTrigger UpdateTrigger { get; }

        /// <summary>
        ///     Gets or sets whether rendering should be paused
        /// </summary>
        bool IsRenderPaused { get; set; }

        /// <summary>
        ///     Adds the given device provider to the <see cref="Surface" />
        /// </summary>
        /// <param name="deviceProvider"></param>
        void AddDeviceProvider(IRGBDeviceProvider deviceProvider);

        /// <summary>
        ///     Occurs when a single device has loaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceLoaded;

        /// <summary>
        ///     Occurs when a single device has reloaded
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceReloaded;

        /// <summary>
        ///     Recalculates the LED group used by the <see cref="BitmapBrush" />
        /// </summary>
        void UpdateSurfaceLedGroup();
    }
}