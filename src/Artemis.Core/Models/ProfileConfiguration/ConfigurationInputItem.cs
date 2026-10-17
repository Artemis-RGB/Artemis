using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile.Configuration;

namespace Artemis.Core;

/// <summary>
///     Represents a base class for configuration items that accept user input.
/// </summary>
public abstract class ConfigurationInputItem : CorePropertyChanged, IConfigurationItem
{
    /// <inheritdoc />
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets the display name of the configuration item.
    /// </summary>
    public required string Name
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the description text that explains the purpose of this configuration item.
    /// </summary>
    public string? Description
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Resets the value of the configuration item to a copy of its default value.
    /// </summary>
    public abstract void ResetToDefault();

    internal abstract string? SerializeValue();
    internal abstract void DeserializeValue(string? json);
    internal abstract void Load(ConfigurationInputItemEntity entity);
    internal abstract void Save(ConfigurationInputItemEntity entity);
    internal abstract void AddDynamicChild(DataModel dataModel, string key, DataModelPropertyAttribute attribute);
    internal abstract void UpdateDynamicChild(DataModel dataModel, DynamicChild dynamicChild);
}

/// <summary>
///     Represents a generic base class for configuration items that accept user input.
/// </summary>
/// <typeparam name="T">The type of the value that this configuration item holds.</typeparam>
public abstract class ConfigurationInputItem<T> : ConfigurationInputItem
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ConfigurationInputItem{T}" /> class.
    /// </summary>
    protected ConfigurationInputItem()
    {
        DropdownValues.CollectionChanged += DropdownValuesOnCollectionChanged;
    }

    /// <summary>
    ///     Gets or sets the current value of the configuration item, as picked by the user.
    /// </summary>
    public T? Value
    {
        get;
        set
        {
            if (IsSameValue(field, value))
                return;

            T? previous = field;
            field = value;
            OnValueReplaced(previous, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the default value of the configuration item, as set by the author of the profile.
    /// </summary>
    public T? DefaultValue
    {
        get;
        set
        {
            if (IsSameValue(field, value))
                return;

            T? previous = field;
            field = value;
            OnValueReplaced(previous, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the collection of dropdown values for this configuration item.
    ///     When populated, the configuration item will be rendered as a dropdown/combo box.
    /// </summary>
    public ObservableCollection<ConfigurationDropdownValue<T>> DropdownValues { get; } = [];

    /// <summary>
    ///     Gets the value that is exposed to the data model when <see cref="Value" /> is <see langword="null" />.
    /// </summary>
    protected virtual T DataModelFallbackValue => default!;

    /// <inheritdoc />
    public override void ResetToDefault()
    {
        Value = CopyValue(DefaultValue);
    }

    /// <summary>
    ///     Determines whether two values are considered the same, if so no change notification is raised.
    /// </summary>
    protected virtual bool IsSameValue(T? a, T? b)
    {
        return EqualityComparer<T?>.Default.Equals(a, b);
    }

    /// <summary>
    ///     Creates a copy of the provided value, override this for mutable reference types.
    /// </summary>
    protected virtual T? CopyValue(T? value)
    {
        return value;
    }

    /// <summary>
    ///     Called when <see cref="Value" /> or <see cref="DefaultValue" /> is replaced by another value.
    /// </summary>
    protected virtual void OnValueReplaced(T? previous, T? current)
    {
    }

    // Forward changes to the dropdown values as a change of the property so that they are picked up like any other change
    private void DropdownValuesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (ConfigurationDropdownValue<T> dropdownValue in e.OldItems)
                dropdownValue.PropertyChanged -= DropdownValueOnPropertyChanged;
        }

        if (e.NewItems != null)
        {
            foreach (ConfigurationDropdownValue<T> dropdownValue in e.NewItems)
                dropdownValue.PropertyChanged += DropdownValueOnPropertyChanged;
        }

        OnPropertyChanged(nameof(DropdownValues));
    }

    private void DropdownValueOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(DropdownValues));
    }

    #region Storage

    internal override string? SerializeValue()
    {
        return SerializeValue(Value);
    }

    internal override void DeserializeValue(string? json)
    {
        if (TryDeserializeValue(json, out T? value))
            Value = value;
        else
            ResetToDefault();
    }

    internal override void Load(ConfigurationInputItemEntity entity)
    {
        Name = entity.Name;
        Description = entity.Description;
        if (TryDeserializeValue(entity.DefaultValue, out T? defaultValue))
            DefaultValue = defaultValue;

        DropdownValues.Clear();
        foreach (ConfigurationDropdownValueEntity dropdownValueEntity in entity.DropdownValues)
        {
            if (TryDeserializeValue(dropdownValueEntity.Value, out T? dropdownValue) && dropdownValue != null)
                DropdownValues.Add(new ConfigurationDropdownValue<T> {DisplayName = dropdownValueEntity.DisplayName, Value = dropdownValue});
        }
    }

    internal override void Save(ConfigurationInputItemEntity entity)
    {
        entity.Id = Id;
        entity.Name = Name;
        entity.Description = Description;
        entity.DefaultValue = SerializeValue(DefaultValue);
        entity.DropdownValues = DropdownValues
            .Select(d => new ConfigurationDropdownValueEntity {DisplayName = d.DisplayName, Value = SerializeValue(d.Value)})
            .ToList();
    }

    private static string? SerializeValue(T? value)
    {
        return value == null ? null : CoreJson.Serialize(value);
    }

    private static bool TryDeserializeValue(string? json, out T? value)
    {
        value = default;
        if (json == null)
            return false;

        try
        {
            value = CoreJson.Deserialize<T>(json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region Data model

    internal override void AddDynamicChild(DataModel dataModel, string key, DataModelPropertyAttribute attribute)
    {
        dataModel.AddDynamicChild(key, Value ?? DataModelFallbackValue, attribute);
    }

    internal override void UpdateDynamicChild(DataModel dataModel, DynamicChild dynamicChild)
    {
        if (dynamicChild is DynamicChild<T> typedDynamicChild)
            typedDynamicChild.Value = Value ?? DataModelFallbackValue;
    }

    #endregion
}
