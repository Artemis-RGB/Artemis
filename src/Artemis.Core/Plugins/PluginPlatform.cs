using System;

namespace Artemis.Core;

/// <summary>
///     Specifies OS platforms a plugin may support.
/// </summary>
[Flags]
public enum PluginPlatform
{
    /// <summary>The Windows platform.</summary>
    Windows = 0,

    /// <summary>The Linux platform.</summary>
    Linux = 1,

    /// <summary>The OSX platform.</summary>
    // ReSharper disable once InconsistentNaming
    OSX = 2
}