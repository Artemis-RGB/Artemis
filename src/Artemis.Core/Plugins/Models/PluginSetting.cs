using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Stylet;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSetting<T>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly PluginInfo _pluginInfo;
        private readonly PluginSettingEntity _pluginSettingEntity;
        private readonly IPluginSettingRepository _pluginSettingRepository;
        private T _value;

        internal PluginSetting(PluginInfo pluginInfo, IPluginSettingRepository pluginSettingRepository, PluginSettingEntity pluginSettingEntity)
        {
            _pluginInfo = pluginInfo;
            _pluginSettingRepository = pluginSettingRepository;
            _pluginSettingEntity = pluginSettingEntity;

            Name = pluginSettingEntity.Name;
            Value = JsonConvert.DeserializeObject<T>(pluginSettingEntity.Value);
        }

        /// <summary>
        ///     The name of the setting, unique to this plugin
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The value of the setting
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnSettingChanged();
            }
        }

        /// <summary>
        ///     Determines whether the setting has been changed
        /// </summary>
        public bool HasChanged => JsonConvert.SerializeObject(Value) != _pluginSettingEntity.Value;

        /// <summary>
        ///     Resets the setting to the last saved value
        /// </summary>
        public void RejectChanges()
        {
            Value = JsonConvert.DeserializeObject<T>(_pluginSettingEntity.Value);
        }

        /// <summary>
        ///     Saves the setting
        /// </summary>
        public void Save()
        {
            if (!HasChanged)
                return;

            _pluginSettingEntity.Value = JsonConvert.SerializeObject(Value);
            _pluginSettingRepository.Save();
        }

        /// <summary>
        ///     Saves the setting asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            if (!HasChanged)
                return;

            _pluginSettingEntity.Value = JsonConvert.SerializeObject(Value);
            await _pluginSettingRepository.SaveAsync();
        }

        public event EventHandler<EventArgs> SettingChanged;

        protected virtual void OnSettingChanged()
        {
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}