using System;
using System.IO;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Models;
using RGB.NET.Core;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to implement your own RGB device
    /// </summary>
    public abstract class Device : Plugin
    {
        public IRGBDeviceProvider DeviceProvider { get; }

        protected Device(PluginInfo pluginInfo, IRGBDeviceProvider deviceProvider) : base(pluginInfo)
        {
            DeviceProvider = deviceProvider ?? throw new ArgumentNullException(nameof(deviceProvider));
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
            }
        }
    }
}