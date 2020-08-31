using System.Collections.Generic;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     <para>This contains all the settings for your plugin. To access a setting use <see cref="GetSetting{T}" />.</para>
    ///     <para>To use this class, inject it into the constructor of your plugin.</para>
    /// </summary>
    public class PluginSettings
    {
        private readonly PluginInfo _pluginInfo;
        private readonly IPluginRepository _pluginRepository;
        private readonly Dictionary<string, object> _settingEntities;

        internal PluginSettings(PluginInfo pluginInfo, IPluginRepository pluginRepository)
        {
            _pluginInfo = pluginInfo;
            _pluginRepository = pluginRepository;
            _settingEntities = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Gets the setting with the provided name. If the setting does not exist yet, it is created.
        /// </summary>
        /// <typeparam name="T">The type of the setting, can be any serializable type</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="defaultValue">The default value to use if the setting does not exist yet</param>
        /// <returns></returns>
        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default)
        {
            lock (_settingEntities)
            {
                // Return cached value if available
                if (_settingEntities.ContainsKey(name))
                    return (PluginSetting<T>) _settingEntities[name];
                // Try to find in database
                var settingEntity = _pluginRepository.GetSettingByNameAndGuid(name, _pluginInfo.Guid);
                // If not found, create a new one
                if (settingEntity == null)
                {
                    settingEntity = new PluginSettingEntity {Name = name, PluginGuid = _pluginInfo.Guid, Value = JsonConvert.SerializeObject(defaultValue)};
                    _pluginRepository.AddSetting(settingEntity);
                }

                var pluginSetting = new PluginSetting<T>(_pluginInfo, _pluginRepository, settingEntity);

                // This overrides null with the default value, I'm not sure if that's desirable because you
                // might expect something to go null and you might not
                // if (pluginSetting.Value == null && defaultValue != null)
                //    pluginSetting.Value = defaultValue;

                _settingEntities.Add(name, pluginSetting);
                return pluginSetting;
            }
        }
    }
}