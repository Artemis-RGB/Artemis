using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSettings
    {
        private readonly PluginInfo _pluginInfo;
        private readonly ISettingRepository _settingRepository;
        private readonly Dictionary<string, SettingEntity> _settingEntities;

        public PluginSettings(PluginInfo pluginInfo, ISettingRepository settingRepository)
        {
            _pluginInfo = pluginInfo;
            _settingRepository = settingRepository;
            _settingEntities = settingRepository.GetByPluginGuid(_pluginInfo.Guid).ToDictionary(se => se.Name);
        }

        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default(T))
        {
            if (_settingEntities.ContainsKey(name))
                return new PluginSetting<T>(_pluginInfo, _settingRepository, _settingEntities[name]);

            var settingEntity = new SettingEntity {Name = name, PluginGuid = _pluginInfo.Guid, Value = JsonConvert.SerializeObject(defaultValue)};
            _settingRepository.Add(settingEntity);
            _settingRepository.Save();

            _settingEntities.Add(name, settingEntity);
            return GetSetting(name, defaultValue);
        }
    }
}