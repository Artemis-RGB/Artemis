using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Artemis.Core;

/// <summary>
/// Represents basic info about a plugin and contains a reference to the instance of said plugin
/// </summary>
public class PluginInfo : IPrerequisitesSubject
{
    [JsonConstructor]
    internal PluginInfo()
    {
    }

    /// <summary>
    /// The plugins GUID
    /// </summary>
    [JsonRequired]
    [JsonInclude]
    public Guid Guid { get; internal init; }

    /// <summary>
    /// The name of the plugin
    /// </summary>
    [JsonRequired]
    [JsonInclude]
    public string Name { get; internal init; } = null!;

    /// <summary>
    /// The version of the plugin
    /// </summary>
    [JsonRequired]
    [JsonInclude]
    public string Version { get; internal init; } = null!;

    /// <summary>
    /// The main entry DLL, should contain a class implementing Plugin
    /// </summary>
    [JsonRequired]
    [JsonInclude]
    public string Main { get; internal init; } = null!;

    /// <summary>
    /// A short description of the plugin
    /// </summary>
    [JsonInclude]
    public string? Description { get; internal init; }

    /// <summary>
    /// Gets or sets the author of this plugin
    /// </summary>
    [JsonInclude]
    public string? Author { get; internal init; }

    /// <summary>
    /// Gets or sets the website of this plugin or its author
    /// </summary>
    [JsonInclude]
    public Uri? Website { get; internal init; }

    /// <summary>
    /// Gets or sets the repository of this plugin
    /// </summary>
    [JsonInclude]
    public Uri? Repository { get; internal init; }

    /// <summary>
    /// Gets or sets the help page of this plugin
    /// </summary>
    [JsonInclude]
    public Uri? HelpPage { get; internal init; }

    /// <summary>
    /// Gets or sets the help page of this plugin
    /// </summary>
    [JsonInclude]
    public Uri? License { get; internal init; }

    /// <summary>
    /// Gets or sets the author of this plugin
    /// </summary>
    [JsonInclude]
    public string? LicenseName { get; internal init; }

    /// <summary>
    /// The plugins display icon that's shown in the settings see <see href="https://materialdesignicons.com" /> for
    /// available icons
    /// </summary>
    [JsonInclude]
    public string? Icon { get; internal init; }

    /// <summary>
    /// Gets a boolean indicating whether this plugin requires elevated admin privileges
    /// </summary>
    [JsonInclude]
    public bool RequiresAdmin { get; internal init; }

    /// <summary>
    /// Gets or sets a boolean indicating whether hot reloading this plugin is supported
    /// </summary>
    [JsonInclude]
    public bool HotReloadSupported { get; internal init; } = true;

    /// <summary>
    /// Gets
    /// </summary>
    [JsonInclude]
    public PluginPlatform? Platforms { get; internal init; }

    /// <summary>
    /// Gets the API version the plugin was built for
    /// </summary>
    [JsonInclude]
    public Version? Api { get; internal init; } = new(1, 0, 0);

    /// <summary>
    /// Gets the minimum version of Artemis required by this plugin
    /// </summary>
    public Version? MinimumVersion { get; internal init; } = new(1, 0, 0);

    /// <summary>
    /// Gets the plugin this info is associated with
    /// </summary>
    [JsonIgnore]
    public Plugin Plugin { get; internal set; } = null!;

    /// <summary>
    /// Gets a string representing either a full path pointing to an svg or the markdown icon
    /// </summary>
    [JsonIgnore]
    public string? ResolvedIcon => Icon == null ? null : Icon.Contains('.') ? Plugin.ResolveRelativePath(Icon) : Icon;

    /// <summary>
    /// Gets a boolean indicating whether this plugin is compatible with the current operating system and API version
    /// </summary>
    [JsonIgnore]
    public bool IsCompatible => Platforms.MatchesCurrentOperatingSystem() && Api != null && Api.Major >= Constants.PluginApiVersion && MatchesMinimumVersion();

    /// <inheritdoc />
    [JsonIgnore]
    public List<PluginPrerequisite> Prerequisites { get; } = new();

    /// <inheritdoc />
    [JsonIgnore]
    public IEnumerable<PluginPrerequisite> PlatformPrerequisites => Prerequisites.Where(p => p.AppliesToPlatform());

    [JsonIgnore]
    internal string PreferredPluginDirectory => $"{Main.Split(".dll")[0].Replace("/", "").Replace("\\", "")}-{Guid.ToString().Substring(0, 8)}";

    /// <inheritdoc />
    public bool ArePrerequisitesMet()
    {
        return PlatformPrerequisites.All(p => p.IsMet());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} v{Version} - {Guid}";
    }

    private bool MatchesMinimumVersion()
    {
        if (Constants.CurrentVersion == "local")
            return true;

        Version currentVersion = new(Constants.CurrentVersion);
        return currentVersion >= MinimumVersion;
    }

    /// <summary>
    ///     Returns a boolean indicating whether this plugin info matches the provided search string
    /// </summary>
    /// <param name="search">The search string to match</param>
    /// <returns>A boolean indicating whether this plugin info matches the provided search string</returns>
    public bool MatchesSearch(string search)
    {
        return Name.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
               (Description != null && Description.Contains(search, StringComparison.InvariantCultureIgnoreCase));
    }
}