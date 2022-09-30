using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Artemis.Core.JsonConverters;
using Artemis.Core.Services;
using Artemis.Core.Services.Core;
using Artemis.Core.SkiaSharp;
using Newtonsoft.Json;

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
    ///     The full path to the Artemis plugins folder
    /// </summary>
    public static readonly string PluginsFolder = Path.Combine(DataFolder, "Plugins");

    /// <summary>
    ///     The full path to the Artemis user layouts folder
    /// </summary>
    public static readonly string LayoutsFolder = Path.Combine(DataFolder, "User Layouts");

    /// <summary>
    ///     The current API version for plugins
    /// </summary>
    public static readonly int PluginApiVersion = CoreAssembly.GetName().Version.Major;

    /// <summary>
    ///     The plugin info used by core components of Artemis
    /// </summary>
    public static readonly PluginInfo CorePluginInfo = new()
    {
        Guid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Artemis Core", Version = new Version(2, 0)
    };

    /// <summary>
    ///     The build information related to the currently running Artemis build
    ///     <para>Information is retrieved from <c>buildinfo.json</c></para>
    /// </summary>
    public static readonly BuildInfo BuildInfo = File.Exists(Path.Combine(ApplicationFolder, "buildinfo.json"))
        ? JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText(Path.Combine(ApplicationFolder, "buildinfo.json")))!
        : new BuildInfo
        {
            IsLocalBuild = true,
            BuildId = 1337,
            BuildNumber = 1337,
            SourceBranch = "local",
            SourceVersion = "local"
        };

    /// <summary>
    ///     The plugin used by core components of Artemis
    /// </summary>
    public static readonly Plugin CorePlugin = new(CorePluginInfo, new DirectoryInfo(ApplicationFolder), null);

    internal static readonly CorePluginFeature CorePluginFeature = new() {Plugin = CorePlugin, Profiler = CorePlugin.GetProfiler("Feature - Core")};
    internal static readonly EffectPlaceholderPlugin EffectPlaceholderPlugin = new() {Plugin = CorePlugin, Profiler = CorePlugin.GetProfiler("Feature - Effect Placeholder")};

    internal static JsonSerializerSettings JsonConvertSettings = new()
    {
        Converters = new List<JsonConverter> {new SKColorConverter(), new NumericJsonConverter(), new ForgivingIntConverter()}
    };

    internal static JsonSerializerSettings JsonConvertTypedSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        Converters = new List<JsonConverter> {new SKColorConverter(), new NumericJsonConverter(), new ForgivingIntConverter()}
    };

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

    /// <summary>
    ///     Gets the graphics context to be used for rendering by SkiaSharp. Can be set via
    ///     <see cref="IRgbService.UpdateGraphicsContext" />.
    /// </summary>
    public static IManagedGraphicsContext? ManagedGraphicsContext { get; internal set; }
}