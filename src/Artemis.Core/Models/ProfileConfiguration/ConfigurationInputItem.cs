using System.Collections.ObjectModel;

namespace Artemis.Core;

/// <summary>
/// Represents a generic base class for configuration items that accept user input.
/// </summary>
/// <typeparam name="T">The type of the value that this configuration item holds.</typeparam>
public class ConfigurationInputItem<T> : CorePropertyChanged, IConfigurationItem
{
    /// <summary>
    /// Gets or sets the display name of the configuration item.
    /// </summary>
    public required string Name
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets or sets the description text that explains the purpose of this configuration item.
    /// </summary>
    public string? Description
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets or sets the current value of the configuration item.
    /// </summary>
    public T? Value
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets or sets the collection of dropdown values for this configuration item.
    /// When populated, the configuration item will be rendered as a dropdown/combo box.
    /// </summary>
    public ObservableCollection<ConfigurationDropdownValue<T>>? DropdownValues
    {
        get;
        set
        {
            if (Equals(value, field))
                return;
            field = value;
            OnPropertyChanged();
        }
    } = [];
}