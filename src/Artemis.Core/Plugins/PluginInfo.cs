using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.JsonConverters;
using Newtonsoft.Json;

namespace Artemis.Core;

/// <summary>
///     Represents basic info about a plugin and contains a reference to the instance of said plugin
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PluginInfo : CorePropertyChanged, IPrerequisitesSubject
{
    private Version? _api;
    private string? _author;
    private bool _autoEnableFeatures = true;
    private string? _description;
    private Guid _guid;
    private string? _icon;
    private string _main = null!;
    private string _name = null!;
    private PluginPlatform? _platforms;
    private Plugin _plugin = null!;
    private Uri? _repository;
    private bool _requiresAdmin;
    private Version _version = null!;
    private Uri? _website;

    internal PluginInfo()
    {
    }

    /// <summary>
    ///     The plugins GUID
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Guid Guid
    {
        get => _guid;
        internal set => SetAndNotify(ref _guid, value);
    }

    /// <summary>
    ///     The name of the plugin
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Name
    {
        get => _name;
        internal set => SetAndNotify(ref _name, value);
    }

    /// <summary>
    ///     A short description of the plugin
    /// </summary>
    [JsonProperty]
    public string? Description
    {
        get => _description;
        set => SetAndNotify(ref _description, value);
    }

    /// <summary>
    ///     Gets or sets the author of this plugin
    /// </summary>
    [JsonProperty]
    public string? Author
    {
        get => _author;
        set => SetAndNotify(ref _author, value);
    }

    /// <summary>
    ///     Gets or sets the website of this plugin or its author
    /// </summary>
    [JsonProperty]
    public Uri? Website
    {
        get => _website;
        set => SetAndNotify(ref _website, value);
    }

    /// <summary>
    ///     Gets or sets the repository of this plugin
    /// </summary>
    [JsonProperty]
    public Uri? Repository
    {
        get => _repository;
        set => SetAndNotify(ref _repository, value);
    }

    /// <summary>
    ///     The plugins display icon that's shown in the settings see <see href="https://materialdesignicons.com" /> for
    ///     available icons
    /// </summary>
    [JsonProperty]
    public string? Icon
    {
        get => _icon;
        set => SetAndNotify(ref _icon, value);
    }

    /// <summary>
    ///     The version of the plugin
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Version Version
    {
        get => _version;
        internal set => SetAndNotify(ref _version, value);
    }

    /// <summary>
    ///     The main entry DLL, should contain a class implementing Plugin
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Main
    {
        get => _main;
        internal set => SetAndNotify(ref _main, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this plugin should automatically enable all its features when it is first
    ///     loaded
    /// </summary>
    [DefaultValue(true)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool AutoEnableFeatures
    {
        get => _autoEnableFeatures;
        set => SetAndNotify(ref _autoEnableFeatures, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether this plugin requires elevated admin privileges
    /// </summary>
    [JsonProperty]
    public bool RequiresAdmin
    {
        get => _requiresAdmin;
        internal set => SetAndNotify(ref _requiresAdmin, value);
    }

    /// <summary>
    ///     Gets
    /// </summary>
    [JsonProperty]
    public PluginPlatform? Platforms
    {
        get => _platforms;
        internal set => SetAndNotify(ref _platforms, value);
    }

    /// <summary>
    ///     Gets the API version the plugin was built for
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [JsonConverter(typeof(ForgivingVersionConverter))]
    public Version? Api
    {
        get => _api;
        internal set => SetAndNotify(ref _api, value);
    }

    /// <summary>
    ///     Gets the plugin this info is associated with
    /// </summary>
    public Plugin Plugin
    {
        get => _plugin;
        internal set => SetAndNotify(ref _plugin, value);
    }

    /// <summary>
    ///     Gets a string representing either a full path pointing to an svg or the markdown icon
    /// </summary>
    public string? ResolvedIcon
    {
        get
        {
            if (Icon == null)
                return null;
            return Icon.Contains('.') ? Plugin.ResolveRelativePath(Icon) : Icon;
        }
    }

    /// <summary>
    ///     Gets a boolean indicating whether this plugin is compatible with the current operating system and API version
    /// </summary>
    public bool IsCompatible => Platforms.MatchesCurrentOperatingSystem() && Api != null && Api.Major >= Constants.PluginApiVersion;

    internal string PreferredPluginDirectory => $"{Main.Split(".dll")[0].Replace("/", "").Replace("\\", "")}-{Guid.ToString().Substring(0, 8)}";

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} v{Version} - {Guid}";
    }

    /// <inheritdoc />
    public List<PluginPrerequisite> Prerequisites { get; } = new();

    /// <inheritdoc />
    public IEnumerable<PluginPrerequisite> PlatformPrerequisites => Prerequisites.Where(p => p.AppliesToPlatform());

    /// <inheritdoc />
    public bool ArePrerequisitesMet()
    {
        return PlatformPrerequisites.All(p => p.IsMet());
    }
}