using System;
using System.Diagnostics.CodeAnalysis;
using Artemis.Core.Properties;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a setting tied to a plugin of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The value type of the setting</typeparam>
    public class PluginSetting<T> : CorePropertyChanged
    {
        // TODO: Why? Should have included that...
        // ReSharper disable once NotAccessedField.Local 
        private readonly Plugin _plugin;
        private readonly IPluginRepository _pluginRepository;
        private readonly PluginSettingEntity _pluginSettingEntity;
        private T _value;

        internal PluginSetting(Plugin plugin, IPluginRepository pluginRepository, PluginSettingEntity pluginSettingEntity)
        {
            _plugin = plugin;
            _pluginRepository = pluginRepository;
            _pluginSettingEntity = pluginSettingEntity;

            Name = pluginSettingEntity.Name;
            try
            {
                _value = JsonConvert.DeserializeObject<T>(pluginSettingEntity.Value, Constants.JsonConvertSettings);
            }
            catch (JsonReaderException)
            {
                _value = default!;
            }
        }

        /// <summary>
        ///     The name of the setting, unique to this plugin
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The value of the setting
        /// </summary>
        [AllowNull]
        [CanBeNull]
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;

                _value = value!;
                OnSettingChanged();
                OnPropertyChanged(nameof(Value));

                if (AutoSave)
                    Save();
            }
        }

        /// <summary>
        ///     Determines whether the setting has been changed
        /// </summary>
        public bool HasChanged => JsonConvert.SerializeObject(Value, Constants.JsonConvertSettings) != _pluginSettingEntity.Value;

        /// <summary>
        ///     Gets or sets whether changes must automatically be saved
        ///     <para>Note: When set to <c>true</c> <see cref="HasChanged" /> is always <c>false</c></para>
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        ///     Resets the setting to the last saved value
        /// </summary>
        public void RejectChanges()
        {
            Value = JsonConvert.DeserializeObject<T>(_pluginSettingEntity.Value, Constants.JsonConvertSettings);
        }

        /// <summary>
        ///     Saves the setting
        /// </summary>
        public void Save()
        {
            if (!HasChanged)
                return;

            _pluginSettingEntity.Value = JsonConvert.SerializeObject(Value, Constants.JsonConvertSettings);
            _pluginRepository.SaveSetting(_pluginSettingEntity);
            OnSettingSaved();
        }

        /// <summary>
        ///     Occurs when the value of the setting has been changed
        /// </summary>
        public event EventHandler? SettingChanged;

        /// <summary>
        ///     Occurs when the value of the setting has been saved
        /// </summary>
        public event EventHandler? SettingSaved;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Value)}: {Value}, {nameof(HasChanged)}: {HasChanged}";
        }

        /// <summary>
        ///     Invokes the <see cref="SettingChanged" /> event
        /// </summary>
        protected internal virtual void OnSettingChanged()
        {
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Invokes the <see cref="OnSettingSaved" /> event
        /// </summary>
        protected internal virtual void OnSettingSaved()
        {
            SettingSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}