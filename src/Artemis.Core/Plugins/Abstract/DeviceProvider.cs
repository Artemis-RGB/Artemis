using System;
using System.IO;
using Artemis.Core.Extensions;
using Ninject;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to implement and register your own device provider
    /// </summary>
    public abstract class DeviceProvider : Plugin
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
        public ILogger Logger { get; set; }

        /// <inheritdoc />
        public override void DisablePlugin()
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
                    e.FinalPath = Path.Combine(PluginInfo.Directory.FullName, e.RelativePart, e.FileName);
                else if (e.RelativePath != null)
                    e.FinalPath = Path.Combine(PluginInfo.Directory.FullName, e.RelativePath);

                var deviceInfo = ((IRGBDevice) sender).DeviceInfo;
                if (e.FileName != null && !File.Exists(e.FinalPath))
                {
                    Logger?.Information("Couldn't find a layout for device {deviceName}, model {deviceModel} at {filePath}",
                        deviceInfo.DeviceName, deviceInfo.Model, e.FinalPath);
                }
            }
        }
    }
}