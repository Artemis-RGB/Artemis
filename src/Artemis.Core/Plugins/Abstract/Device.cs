using System;
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
    }
}