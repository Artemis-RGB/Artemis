namespace Artemis.Core
{
    /// <summary>
    ///     Represents a device input identifier used by a specific <see cref="Services.InputProvider" /> to identify the
    ///     device
    /// </summary>
    public class ArtemisDeviceInputIdentifier
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ArtemisDeviceInputIdentifier" /> class
        /// </summary>
        /// <param name="inputProvider">
        ///     The full type and namespace of the <see cref="Services.InputProvider" /> this identifier is
        ///     used by
        /// </param>
        /// <param name="identifier">A value used to identify the device</param>
        internal ArtemisDeviceInputIdentifier(string inputProvider, object identifier)
        {
            InputProvider = inputProvider;
            Identifier = identifier;
        }

        /// <summary>
        ///     Gets or sets the full type and namespace of the <see cref="Services.InputProvider" /> this identifier is used by
        /// </summary>
        public string InputProvider { get; set; }

        /// <summary>
        ///     Gets or sets a value used to identify the device
        /// </summary>
        public object Identifier { get; set; }
    }
}