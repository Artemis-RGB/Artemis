using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSettings
    {
        private readonly PluginInfo _pluginInfo;
        private readonly IPluginSettingRepository _pluginSettingRepository;
        private readonly Dictionary<string, PluginSettingEntity> _settingEntities;

        internal PluginSettings(PluginInfo pluginInfo, IPluginSettingRepository pluginSettingRepository)
        {
            _pluginInfo = pluginInfo;
            _pluginSettingRepository = pluginSettingRepository;
            _settingEntities = pluginSettingRepository.GetByPluginGuid(_pluginInfo.Guid).ToDictionary(se => se.Name);
        }

        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default(T))
        {
            if (_settingEntities.ContainsKey(name))
                return new PluginSetting<T>(_pluginInfo, _pluginSettingRepository, _settingEntities[name]);

            var settingEntity = new PluginSettingEntity {Name = name, PluginGuid = _pluginInfo.Guid, Value = JsonConvert.SerializeObject(defaultValue)};
            _pluginSettingRepository.Add(settingEntity);
            _pluginSettingRepository.Save();
             
            _settingEntities.Add(name, settingEntity);
            return new PluginSetting<T>(_pluginInfo, _pluginSettingRepository, _settingEntities[name]);
        }
    }
}