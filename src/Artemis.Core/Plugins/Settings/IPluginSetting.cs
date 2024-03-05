using System;
using System.Text.Json;

namespace Artemis.Core;

/// <summary>
///     Represents a setting tied to a plugin
/// </summary>
public interface IPluginSetting
{
    /// <summary>
    ///  The JSON serializer options used when serializing settings
    /// </summary>
    protected static readonly JsonSerializerOptions SerializerOptions = CoreJson.GetJsonSerializerOptions();

    /// <summary>
    ///     The name of the setting, unique to this plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Determines whether the setting has been changed
    /// </summary>
    bool HasChanged { get; }

    /// <summary>
    ///     Gets or sets whether changes must automatically be saved
    ///     <para>Note: When set to <c>true</c> <see cref="HasChanged" /> is always <c>false</c></para>
    /// </summary>
    bool AutoSave { get; set; }

    /// <summary>
    ///     Resets the setting to the last saved value
    /// </summary>
    void RejectChanges();

    /// <summary>
    ///     Saves the setting
    /// </summary>
    void Save();

    /// <summary>
    ///     Occurs when the value of the setting has been changed
    /// </summary>
    event EventHandler? SettingChanged;

    /// <summary>
    ///     Occurs when the value of the setting has been saved
    /// </summary>
    event EventHandler? SettingSaved;
}