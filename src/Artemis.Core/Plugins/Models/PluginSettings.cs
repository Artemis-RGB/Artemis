using Artemis.Storage.Repositories;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSettings
    {
        private readonly PluginInfo _pluginInfo;
        private readonly SettingRepository _settingRepository;

        internal PluginSettings(PluginInfo pluginInfo, SettingRepository settingRepository)
        {
            _pluginInfo = pluginInfo;
            _settingRepository = settingRepository;
        }

        public bool HasSettingChanged(string settingName)
        {
            return false;
        }

        public bool HasAnySettingChanged()
        {
            return false;
        }
    }
}