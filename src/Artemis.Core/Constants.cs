using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Artemis.Core.JsonConverters;
using Artemis.Storage.Entities.Plugins;

namespace Artemis.Core;

/// <summary>
///     A few useful constant values
/// </summary>
public static class Constants
{
    /// <summary>
    ///     The Artemis.Core assembly
    /// </summary>
    public static readonly Assembly CoreAssembly = typeof(Constants).Assembly;

    /// <summary>
    ///     The full path to the Artemis application folder
    /// </summary>
    public static readonly string ApplicationFolder = Path.GetDirectoryName(typeof(Constants).Assembly.Location)!;

    /// <summary>
    ///     The full path to the Artemis executable
    /// </summary>
    public static readonly string ExecutablePath = Utilities.GetCurrentLocation();

    /// <summary>
    ///     The base path for Artemis application data folder
    /// </summary>
    public static readonly string BaseFolder = Environment.GetFolderPath(OperatingSystem.IsWindows()
        ? Environment.SpecialFolder.CommonApplicationData
        : Environment.SpecialFolder.LocalApplicationData);

    /// <summary>
    ///     The full path to the Artemis data folder
    /// </summary>
    public static readonly string DataFolder = Path.Combine(BaseFolder, "Artemis");

    /// <summary>
    ///     The full path to the Artemis logs folder
    /// </summary>
    public static readonly string LogsFolder = Path.Combine(DataFolder, "Logs");

    /// <summary>
    ///     The full path to the Artemis logs folder
    /// </summary>
    public static readonly string UpdatingFolder = Path.Combine(DataFolder, "updating");

    /// <summary>
    ///     The full path to the Artemis plugins folder
    /// </summary>
    public static readonly string PluginsFolder = Path.Combine(DataFolder, "Plugins");

    /// <summary>
    ///     The full path to the Artemis user layouts folder
    /// </summary>
    public static readonly string LayoutsFolder = Path.Combine(DataFolder, "User Layouts");

    /// <summary>
    ///     The full path to the Artemis user layouts folder
    /// </summary>
    public static readonly string WorkshopFolder = Path.Combine(DataFolder, "workshop");

    /// <summary>
    ///     The current API version for plugins
    /// </summary>
    public static readonly int PluginApiVersion = int.Parse(CoreAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "PluginApiVersion")?.Value ??
                                                            throw new InvalidOperationException("Cannot find PluginApiVersion metadata in assembly"));

    /// <summary>
    ///     The current version of the application
    /// </summary>
    public static readonly string CurrentVersion = CoreAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.StartsWith("1.0.0")
        ? "local"
        : CoreAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    /// <summary>
    ///     The plugin info used by core components of Artemis
    /// </summary>
    public static readonly PluginInfo CorePluginInfo = new()
    {
        Guid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Artemis Core", Version = CurrentVersion
    };

    /// <summary>
    ///     The plugin used by core components of Artemis
    /// </summary>
    public static readonly Plugin CorePlugin = new(CorePluginInfo, new DirectoryInfo(ApplicationFolder), new PluginEntity(){PluginGuid = CorePluginInfo.Guid}, false);

    /// <summary>
    ///     A read-only collection containing all primitive numeric types
    /// </summary>
    public static IReadOnlyCollection<Type> NumberTypes = new List<Type>
    {
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal)
    };

    /// <summary>
    ///     A read-only collection containing all primitive integral numeric types
    /// </summary>
    public static IReadOnlyCollection<Type> IntegralNumberTypes = new List<Type>
    {
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong)
    };

    /// <summary>
    ///     A read-only collection containing all primitive floating-point numeric types
    /// </summary>
    public static IReadOnlyCollection<Type> FloatNumberTypes = new List<Type>
    {
        typeof(float),
        typeof(double),
        typeof(decimal)
    };

    /// <summary>
    ///     Gets the startup arguments provided to the application
    /// </summary>
    public static ReadOnlyCollection<string> StartupArguments { get; set; } = null!;

    public static string? GetStartupRoute()
    {
        return StartupArguments.FirstOrDefault(a => a.StartsWith("--route=artemis://"))?.Split("--route=artemis://")[1];
    }
    
    internal static readonly CorePluginFeature CorePluginFeature = new() {Plugin = CorePlugin, Profiler = CorePlugin.GetProfiler("Feature - Core")};
    internal static readonly EffectPlaceholderPlugin EffectPlaceholderPlugin = new() {Plugin = CorePlugin, Profiler = CorePlugin.GetProfiler("Feature - Effect Placeholder")};
    internal static readonly JsonSerializerOptions JsonConvertSettings = new() {Converters = {new SKColorConverter(), new NumericJsonConverter()}};
}