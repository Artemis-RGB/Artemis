using System;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginSetting<T>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly PluginInfo _pluginInfo;
        private readonly IPluginRepository _pluginRepository;
        private readonly PluginSettingEntity _pluginSettingEntity;
        private T _value;

        internal PluginSetting(PluginInfo pluginInfo, IPluginRepository pluginRepository, PluginSettingEntity pluginSettingEntity)
        {
            _pluginInfo = pluginInfo;
            _pluginRepository = pluginRepository;
            _pluginSettingEntity = pluginSettingEntity;

            Name = pluginSettingEntity.Name;
            try
            {
                Value = JsonConvert.DeserializeObject<T>(pluginSettingEntity.Value);
            }
            catch (JsonReaderException)
            {
                Value = default;
            }
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
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnSettingChanged();
                }
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
            _pluginRepository.SaveSetting(_pluginSettingEntity);
        }

        public event EventHandler<EventArgs> SettingChanged;

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Value)}: {Value}, {nameof(HasChanged)}: {HasChanged}";
        }

        protected virtual void OnSettingChanged()
        {
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}