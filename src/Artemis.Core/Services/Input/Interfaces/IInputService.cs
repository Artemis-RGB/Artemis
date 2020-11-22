using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to interact with keyboard and mice input events
    /// </summary>
    public interface IInputService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Adds an input provided
        /// </summary>
        /// <param name="inputProvider">The input provider the add</param>
        void AddInputProvider(InputProvider inputProvider);

        /// <summary>
        ///     Removes an input provided
        /// </summary>
        /// <param name="inputProvider">The input provider the remove</param>
        void RemoveInputProvider(InputProvider inputProvider);

        /// <summary>
        ///     Identifies the provided <paramref name="device" /> by assigning the next received device identification to it
        /// </summary>
        /// <param name="device">The device to identify</param>
        void IdentifyDevice(ArtemisDevice device);

        /// <summary>
        ///     Cancels identifying the device last passed to <see cref="IdentifyDevice" />
        /// </summary>
        void StopIdentify();

        #region Events

        /// <summary>
        ///     Occurs whenever a key on a keyboard was pressed or released
        /// </summary>
        event EventHandler<KeyboardKeyUpDownEventArgs> KeyboardKeyUpDown;

        /// <summary>
        ///     Occurs whenever a key on a keyboard was pressed
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyboardKeyDown;

        /// <summary>
        ///     Occurs whenever a key on a keyboard was released
        /// </summary>
        event EventHandler<KeyboardEventArgs> KeyboardKeyUp;

        /// <summary>
        ///     Occurs when a device has been identified after calling <see cref="IdentifyDevice" />
        /// </summary>
        public event EventHandler DeviceIdentified;

        #endregion

        #region Identification

        /// <summary>
        ///     Attempts to identify the device using the provided <paramref name="identifier" />
        /// </summary>
        /// <param name="provider">The input provider to identify the device for</param>
        /// <param name="identifier">The value to use to identify the device</param>
        /// <param name="fallbackType">A device type to fall back to if no match is found</param>
        /// <returns>If found, the Artemis device matching the provider and identifier</returns>
        ArtemisDevice? GetDeviceByIdentifier(InputProvider provider, object identifier, InputFallbackDeviceType fallbackType);

        /// <summary>
        ///     Clears the identifier cache
        /// </summary>
        void BustIdentifierCache();

        #endregion
    }
}