using System;
using System.IO;
using Ninject;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.DeviceProviders
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to implement and register your own device provider
    /// </summary>
    public abstract class DeviceProvider : PluginFeature
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DeviceProvider" /> class
        /// </summary>
        /// <param name="rgbDeviceProvider"></param>
        protected DeviceProvider(IRGBDeviceProvider rgbDeviceProvider)
        {
            RgbDeviceProvider = rgbDeviceProvider ?? throw new ArgumentNullException(nameof(rgbDeviceProvider));
        }

        /// <summary>
        ///     The RGB.NET device provider backing this Artemis device provider
        /// </summary>
        public IRGBDeviceProvider RgbDeviceProvider { get; }

        /// <summary>
        ///     TODO: Make internal while still injecting.
        ///     A logger used by the device provider internally, ignore this
        /// </summary>
        [Inject]
        public ILogger? Logger { get; set; }

        /// <summary>
        ///     A boolean indicating whether this device provider detects the physical layout of connected keyboards.
        ///     <para>
        ///         Note: <see cref="DetectDeviceLayout" /> is only called when this or <see cref="CanDetectLogicalLayout" />
        ///         is <see langword="true" />.
        ///     </para>
        /// </summary>
        public bool CanDetectPhysicalLayout { get; protected set; }

        /// <summary>
        ///     A boolean indicating whether this device provider detects the logical layout of connected keyboards
        ///     <para>
        ///         Note: <see cref="DetectDeviceLayout" /> is only called when this or <see cref="CanDetectPhysicalLayout" />
        ///         is <see langword="true" />.
        ///     </para>
        /// </summary>
        public bool CanDetectLogicalLayout { get; protected set; }

        /// <inheritdoc />
        public override void Disable()
        {
            // Does not happen with device providers, they require Artemis to restart
        }

        /// <summary>
        ///     Loads a layout for the specified device and wraps it in an <see cref="ArtemisLayout" />
        /// </summary>
        /// <param name="rgbDevice">The device to load the layout for</param>
        /// <returns>The resulting Artemis layout</returns>
        public virtual ArtemisLayout LoadLayout(ArtemisDevice rgbDevice)
        {
            string layoutDir = Path.Combine(Plugin.Directory.FullName, "Layouts");
            string filePath = Path.Combine(
                layoutDir,
                rgbDevice.RgbDevice.DeviceInfo.Manufacturer,
                rgbDevice.RgbDevice.DeviceInfo.DeviceType.ToString(),
                rgbDevice.GetLayoutFileName()
            );
            return new ArtemisLayout(filePath);
        }

        /// <summary>
        ///     Called when a specific RGB device's logical and physical layout must be detected
        ///     <para>
        ///         Note: Only called when <see cref="CanDetectPhysicalLayout" /> or <see cref="CanDetectLogicalLayout" /> is <see langword="true" />.
        ///     </para>
        /// </summary>
        /// <param name="rgbDevice">The device to detect the layout for, always a keyboard</param>
        public virtual void DetectDeviceLayout(ArtemisDevice rgbDevice)
        {
        }
    }
}