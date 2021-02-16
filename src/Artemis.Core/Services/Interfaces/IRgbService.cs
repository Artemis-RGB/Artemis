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
        ///     Gets a read-only collection containing all enabled devices
        /// </summary>
        IReadOnlyCollection<ArtemisDevice> EnabledDevices { get; }

        /// <summary>
        ///     Gets a read-only collection containing all registered devices
        /// </summary>
        IReadOnlyCollection<ArtemisDevice> Devices { get; }

        /// <summary>
        ///     Gets a dictionary containing all <see cref="ArtemisLed" />s on the surface with their corresponding RGB.NET
        ///     <see cref="Led" /> as key
        /// </summary>
        IReadOnlyDictionary<Led, ArtemisLed> LedMap { get; }

        /// <summary>
        ///     Gets or sets the RGB surface rendering is performed on
        /// </summary>
        RGBSurface Surface { get; set; }

        /// <summary>
        ///     Gets the bitmap brush used to convert the rendered frame to LED-colors
        /// </summary>
        BitmapBrush? BitmapBrush { get; }

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
        ///     Removes the given device provider from the <see cref="Surface" />
        /// </summary>
        /// <param name="deviceProvider"></param>
        void RemoveDeviceProvider(IRGBDeviceProvider deviceProvider);

        /// <summary>
        ///     Applies auto-arranging logic to the surface
        /// </summary>
        void AutoArrangeDevices();

        /// <summary>
        ///     Applies the best available layout for the given <see cref="ArtemisDevice" />
        /// </summary>
        /// <param name="device">The device to apply the best available layout to</param>
        /// <returns>The layout that was applied to the device</returns>
        ArtemisLayout ApplyBestDeviceLayout(ArtemisDevice device);

        /// <summary>
        ///     Apples the provided <see cref="ArtemisLayout" /> to the provided <see cref="ArtemisDevice" />
        /// </summary>
        /// <param name="device"></param>
        /// <param name="layout"></param>
        void ApplyDeviceLayout(ArtemisDevice device, ArtemisLayout layout);

        /// <summary>
        ///     Attempts to retrieve the <see cref="ArtemisDevice" /> that corresponds the provided RGB.NET
        ///     <see cref="IRGBDevice" />
        /// </summary>
        /// <param name="rgbDevice">
        ///     The RGB.NET <see cref="IRGBDevice" /> to find the corresponding <see cref="ArtemisDevice" />
        ///     for
        /// </param>
        /// <returns>If found, the corresponding <see cref="ArtemisDevice" />; otherwise <see langword="null" />.</returns>
        ArtemisDevice? GetDevice(IRGBDevice rgbDevice);

        /// <summary>
        ///     Attempts to retrieve the <see cref="ArtemisLed" /> that corresponds the provided RGB.NET <see cref="Led" />
        /// </summary>
        /// <param name="led">The RGB.NET <see cref="Led" /> to find the corresponding <see cref="ArtemisLed" /> for </param>
        /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
        ArtemisLed? GetLed(Led led);

        /// <summary>
        ///     Saves the configuration of the provided device to persistent storage
        /// </summary>
        /// <param name="artemisDevice"></param>
        void SaveDevice(ArtemisDevice artemisDevice);

        /// <summary>
        ///     Saves the configuration of all current devices to persistent storage
        /// </summary>
        void SaveDevices();

        void EnableDevice(ArtemisDevice device);
        void DisableDevice(ArtemisDevice device);

        /// <summary>
        ///     Occurs when a single device was added
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceAdded;

        /// <summary>
        ///     Occurs when a single device was removed
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceRemoved;

        /// <summary>
        /// Occurs when the surface has had modifications to its LED collection
        /// </summary>
        event EventHandler LedsChanged;
    }
}