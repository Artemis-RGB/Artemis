using System.Collections.Generic;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    /// <summary>
    ///     <para>This contains all the settings for your plugin. To access a setting use <see cref="GetSetting{T}" />.</para>
    ///     <para>To use this class, inject it into the constructor of your plugin.</para>
    /// </summary>
    public class PluginSettings
    {
        private readonly PluginInfo _pluginInfo;
        private readonly IPluginSettingRepository _pluginSettingRepository;
        private readonly Dictionary<string, object> _settingEntities;

        internal PluginSettings(PluginInfo pluginInfo, IPluginSettingRepository pluginSettingRepository)
        {
            _pluginInfo = pluginInfo;
            _pluginSettingRepository = pluginSettingRepository;
            _settingEntities = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Gets the setting with the provided name. If the setting does not exist yet, it is created.
        /// </summary>
        /// <typeparam name="T">The type of the setting, can be any serializable type</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="defaultValue">The default value to use if the setting does not exist yet</param>
        /// <returns></returns>
        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default(T))
        {
            lock (_settingEntities)
            {
                // Return cached value if available
                if (_settingEntities.ContainsKey(name))
                    return (PluginSetting<T>) _settingEntities[name];
                // Try to find in database
                var settingEntity = _pluginSettingRepository.GetByNameAndPluginGuid(name, _pluginInfo.Guid);
                // If not found, create a new one
                if (settingEntity == null)
                {
                    settingEntity = new PluginSettingEntity {Name = name, PluginGuid = _pluginInfo.Guid, Value = JsonConvert.SerializeObject(defaultValue)};
                    _pluginSettingRepository.Add(settingEntity);
                    _pluginSettingRepository.Save();
                }

                var pluginSetting = new PluginSetting<T>(_pluginInfo, _pluginSettingRepository, settingEntity);
                _settingEntities.Add(name, pluginSetting);
                return pluginSetting;
            }
        }
    }
}