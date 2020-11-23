using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for input provider identifier events
    /// </summary>
    public class InputProviderIdentifierEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="InputProviderIdentifierEventArgs" /> class
        /// </summary>
        /// <param name="identifier">A value that can be used to identify this device</param>
        /// <param name="deviceType">The type of device this identifier belongs to</param>
        public InputProviderIdentifierEventArgs(object identifier, InputDeviceType deviceType)
        {
            Identifier = identifier;
            DeviceType = deviceType;
        }

        /// <summary>
        ///     Gets a value that can be used to identify this device
        /// </summary>
        public object Identifier { get; }

        /// <summary>
        ///     Gets the type of device this identifier belongs to
        /// </summary>
        public InputDeviceType DeviceType { get; }
    }
}