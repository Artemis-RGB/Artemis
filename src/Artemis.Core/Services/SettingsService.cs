using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    /// <inheritdoc />
    public class SettingsService : ISettingsService
    {
        private readonly PluginInfo _buildInPluginInfo;
        private readonly IPluginSettingRepository _pluginSettingRepository;
        private readonly Dictionary<string, PluginSettingEntity> _settingEntities;

        internal SettingsService(IPluginSettingRepository pluginSettingRepository)
        {
            _pluginSettingRepository = pluginSettingRepository;
            _buildInPluginInfo = new PluginInfo {Guid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Artemis Core"};

            _settingEntities = pluginSettingRepository.GetByPluginGuid(_buildInPluginInfo.Guid).ToDictionary(se => se.Name);
        }

        public PluginSetting<T> GetSetting<T>(string name, T defaultValue = default(T))
        {
            lock (_settingEntities)
            {
                if (_settingEntities.ContainsKey(name))
                    return new PluginSetting<T>(_buildInPluginInfo, _pluginSettingRepository, _settingEntities[name]);

                var settingEntity = new PluginSettingEntity {Name = name, PluginGuid = _buildInPluginInfo.Guid, Value = JsonConvert.SerializeObject(defaultValue)};
                _pluginSettingRepository.Add(settingEntity);
                _pluginSettingRepository.Save();

                _settingEntities.Add(name, settingEntity);
                return new PluginSetting<T>(_buildInPluginInfo, _pluginSettingRepository, _settingEntities[name]);
            }
        }
    }

    /// <summary>
    ///     <para>A wrapper around plugin settings for internal use.</para>
    ///     <para>Do not inject into a plugin, for plugins inject <see cref="PluginSettings" /> instead.</para>
    /// </summary>
    public interface ISettingsService : IProtectedArtemisService
    {
        PluginSetting<T> GetSetting<T>(string name, T defaultValue = default(T));
    }
}