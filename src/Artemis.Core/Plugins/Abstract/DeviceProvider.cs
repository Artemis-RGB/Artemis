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
        protected DeviceProvider(IRGBDeviceProvider rgbDeviceProvider)
        {
            RgbDeviceProvider = rgbDeviceProvider ?? throw new ArgumentNullException(nameof(rgbDeviceProvider));
        }

        public IRGBDeviceProvider RgbDeviceProvider { get; }

        [Inject]
        public ILogger Logger { get; set; }

        public override void DisablePlugin()
        {
            // Does not happen with device providers, they require Artemis to restart
        }

        protected void ResolveAbsolutePath(Type type, object sender, ResolvePathEventArgs e)
        {
            if (sender.GetType().IsGenericType(type))
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