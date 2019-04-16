using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSettingsContainer
    {
        private readonly PluginInfo _pluginInfo;
        private readonly SettingRepository _settingRepository;
        private Task<List<SettingEntity>> _settings;

        internal PluginSettingsContainer(PluginInfo pluginInfo, SettingRepository settingRepository)
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