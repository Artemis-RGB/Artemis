using System;
using System.Collections.Generic;
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

        internal Dictionary<IRGBDevice, string> DeviceLayoutPaths { get; set; } = new();

        /// <inheritdoc />
        public override void Disable()
        {
            // Does not happen with device providers, they require Artemis to restart
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ResolveAbsolutePath(Type type, object sender, ResolvePathEventArgs e)
        {
            if (sender.GetType() == type || sender.GetType().IsGenericType(type))
            {
                // Start from the plugin directory
                if (e.RelativePart != null && e.FileName != null)
                    e.FinalPath = Path.Combine(Plugin.Directory.FullName, e.RelativePart, e.FileName);
                else if (e.RelativePath != null)
                    e.FinalPath = Path.Combine(Plugin.Directory.FullName, e.RelativePath);

                IRGBDevice device = (IRGBDevice) sender;
                IRGBDeviceInfo deviceInfo = device.DeviceInfo;
                if (e.FileName != null && !File.Exists(e.FinalPath))
                {
                    Logger?.Information("Couldn't find a layout for device {deviceName}, model {deviceModel} at {filePath}",
                        deviceInfo.DeviceName, deviceInfo.Model, e.FinalPath);
                }

                if (e.FileName != null)
                    DeviceLayoutPaths[device] = e.FinalPath;
            }
        }
    }
}