using System;
using System.Collections.Generic;
using Artemis.Core.DeviceProviders;

namespace Artemis.Core;

/// <summary>
///     Provides data about device provider related events
/// </summary>
public class DeviceProviderEventArgs : EventArgs
{
    internal DeviceProviderEventArgs(DeviceProvider deviceProvider, List<ArtemisDevice> devices)
    {
        DeviceProvider = deviceProvider;
        Devices = devices;
    }

    /// <summary>
    ///     Gets the device provider the event is related to.
    /// </summary>
    public DeviceProvider DeviceProvider { get; }

    /// <summary>
    ///     Gets a list of the affected devices.
    /// </summary>
    public List<ArtemisDevice> Devices { get; set; }
}