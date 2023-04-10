using System;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core;

/// <summary>
///     Represents a setting tied to a plugin of type <typeparamref name="T" />
/// </summary>
/// <typeparam name="T">The value type of the setting</typeparam>
public class PluginSetting<T> : CorePropertyChanged, IPluginSetting
{
    private readonly IPluginRepository _pluginRepository;
    private readonly PluginSettingEntity _pluginSettingEntity;
    private T _value;

    internal PluginSetting(IPluginRepository pluginRepository, PluginSettingEntity pluginSettingEntity)
    {
        _pluginRepository = pluginRepository;
        _pluginSettingEntity = pluginSettingEntity;

        Name = pluginSettingEntity.Name;
        try
        {
            _value = CoreJson.DeserializeObject<T>(pluginSettingEntity.Value)!;
        }
        catch (JsonReaderException)
        {
            _value = default!;
        }
    }

    /// <summary>
    ///     The value of the setting
    /// </summary>
    public T? Value
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

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool HasChanged => CoreJson.SerializeObject(Value) != _pluginSettingEntity.Value;

    /// <inheritdoc />
    public bool AutoSave { get; set; }

    /// <inheritdoc />
    public void RejectChanges()
    {
        Value = CoreJson.DeserializeObject<T>(_pluginSettingEntity.Value);
    }

    /// <inheritdoc />
    public void Save()
    {
        if (!HasChanged)
            return;

        _pluginSettingEntity.Value = CoreJson.SerializeObject(Value);
        _pluginRepository.SaveSetting(_pluginSettingEntity);
        OnSettingSaved();
    }

    /// <inheritdoc />
    public event EventHandler? SettingChanged;

    /// <inheritdoc />
    public event EventHandler? SettingSaved;
}