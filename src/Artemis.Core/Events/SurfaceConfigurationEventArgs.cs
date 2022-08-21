using System;
using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
///     Provides data about device configuration related events
/// </summary>
public class SurfaceConfigurationEventArgs : EventArgs
{
    internal SurfaceConfigurationEventArgs(List<ArtemisDevice> devices)
    {
        Devices = devices;
    }

    /// <summary>
    ///     Gets the current list of devices
    /// </summary>
    public List<ArtemisDevice> Devices { get; }
}