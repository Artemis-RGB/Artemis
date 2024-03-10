﻿using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Services;

/// <inheritdoc />
internal class SettingsService : ISettingsService
{
    private readonly PluginSettings _pluginSettings;

    public SettingsService(IPluginRepository pluginRepository)
    {
        _pluginSettings = new PluginSettings(Constants.CorePlugin, pluginRepository);
    }

    public PluginSetting<T> GetSetting<T>(string name, T? defaultValue = default)
    {
        return _pluginSettings.GetSetting(name, defaultValue);
    }

    /// <inheritdoc />
    public void SaveAllSettings()
    {
        _pluginSettings.SaveAllSettings();
    }
}

/// <summary>
///     <para>A wrapper around plugin settings for miscellaneous use outside plugins</para>
///     <para>Do not inject into a plugin, for plugins inject <see cref="PluginSettings" /> instead.</para>
/// </summary>
public interface ISettingsService : IProtectedArtemisService
{
    /// <summary>
    ///     Gets the setting with the provided name. If the setting does not exist yet, it is created.
    /// </summary>
    /// <typeparam name="T">The type of the setting, can be any serializable type</typeparam>
    /// <param name="name">The name of the setting, may not be longer than 128 characters</param>
    /// <param name="defaultValue">The default value to use if the setting does not exist yet</param>
    /// <returns></returns>
    PluginSetting<T> GetSetting<T>(string name, T? defaultValue = default);

    /// <summary>
    ///     Saves all settings, obviously
    /// </summary>
    void SaveAllSettings();
}