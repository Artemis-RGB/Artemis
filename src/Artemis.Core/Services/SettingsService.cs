using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Services
{
    /// <inheritdoc />
    public class SettingsService : ISettingsService
    {
        private readonly PluginSettings _pluginSettings;

        internal SettingsService(IPluginRepository pluginRepository)
        {
            _pluginSettings = new PluginSettings(Constants.CorePluginInfo, pluginRepository);
        }

        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default)
        {
            return _pluginSettings.GetSetting(name, defaultValue);
        }
    }

    /// <summary>
    ///     <para>A wrapper around plugin settings for internal use.</para>
    ///     <para>Do not inject into a plugin, for plugins inject <see cref="PluginSettings" /> instead.</para>
    /// </summary>
    public interface ISettingsService : IProtectedArtemisService
    {
        PluginSetting<T> GetSetting<T>(string name, T defaultValue = default);
    }
}