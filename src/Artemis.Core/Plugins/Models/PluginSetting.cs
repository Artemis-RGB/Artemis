using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSetting<T>
    {
        private readonly PluginInfo _pluginInfo;
        private readonly SettingEntity _settingEntity;
        private readonly SettingRepository _settingRepository;

        internal PluginSetting(PluginInfo pluginInfo, SettingRepository settingRepository, SettingEntity settingEntity)
        {
            _pluginInfo = pluginInfo;
            _settingRepository = settingRepository;
            _settingEntity = settingEntity;

            Name = settingEntity.Name;
            Value = JsonConvert.DeserializeObject<T>(settingEntity.Value);
        }

        /// <summary>
        ///     The name of the setting, unique to this plugin
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The value of the setting
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        ///     Determines whether the setting has been changed
        /// </summary>
        public bool HasChanged => JsonConvert.SerializeObject(Value) != _settingEntity.Value;

        /// <summary>
        ///     Resets the setting to the last saved value
        /// </summary>
        public void RejectChanges()
        {
            Value = JsonConvert.DeserializeObject<T>(_settingEntity.Value);
        }

        /// <summary>
        ///     Saves the setting
        /// </summary>
        public void Save()
        {
            _settingEntity.Value = JsonConvert.SerializeObject(Value);
            _settingRepository.Save();
        }

        /// <summary>
        ///     Saves the setting asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            _settingEntity.Value = JsonConvert.SerializeObject(Value);
            await _settingRepository.SaveAsync();
        }
    }
}