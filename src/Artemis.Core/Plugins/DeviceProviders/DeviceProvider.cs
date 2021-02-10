using System;
using System.IO;
using System.Linq;
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
        ///     A boolean indicating whether this device provider detects the physical layout of connected keyboards
        /// </summary>
        public bool CanDetectPhysicalLayout { get; protected set; }

        /// <summary>
        ///     A boolean indicating whether this device provider detects the logical layout of connected keyboards
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
        public virtual ArtemisLayout LoadLayout(IRGBDevice rgbDevice)
        {
            // Take out invalid file name chars, may not be perfect but neither are you
            string model = Path.GetInvalidFileNameChars().Aggregate(rgbDevice.DeviceInfo.Model, (current, c) => current.Replace(c, '-'));
            string layoutDir = Path.Combine(Plugin.Directory.FullName, "Layouts");
            string filePath;
            // if (rgbDevice.DeviceInfo is IPhysicalLayoutDeviceInfo)
            // {
            //     filePath = Path.Combine(
            //         layoutDir,
            //         rgbDevice.DeviceInfo.Manufacturer,
            //         rgbDevice.DeviceInfo.DeviceType.ToString(),
            //         model,
            //         keyboard.DeviceInfo.
            //     ) + ".xml";
            // }
            // else
            // {
            filePath = Path.Combine(
                layoutDir,
                rgbDevice.DeviceInfo.Manufacturer,
                rgbDevice.DeviceInfo.DeviceType.ToString(),
                model
            ) + ".xml";
            // }

            return new ArtemisLayout(filePath);
        }
    }
}