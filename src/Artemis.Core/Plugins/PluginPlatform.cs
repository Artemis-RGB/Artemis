using System;
using System.Text.Json.Serialization;

namespace Artemis.Core;

/// <summary>
///     Specifies OS platforms a plugin may support.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PluginPlatform
{
    /// <summary>The Windows platform.</summary>
    Windows = 1,

    /// <summary>The Linux platform.</summary>
    Linux = 2,

    /// <summary>The OSX platform.</summary>
    // ReSharper disable once InconsistentNaming
    OSX = 4
}

internal static class PluginPlatformExtensions
{
    /// <summary>
    ///     Determines whether the provided platform matches the current operating system.
    /// </summary>
    internal static bool MatchesCurrentOperatingSystem(this PluginPlatform? platform)
    {
        if (platform == null)
            return true;

        if (OperatingSystem.IsWindows())
            return platform.Value.HasFlag(PluginPlatform.Windows);
        if (OperatingSystem.IsLinux())
            return platform.Value.HasFlag(PluginPlatform.Linux);
        if (OperatingSystem.IsMacOS())
            return platform.Value.HasFlag(PluginPlatform.OSX);
        return false;
    }
}