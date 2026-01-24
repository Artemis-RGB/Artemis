using System.Collections.ObjectModel;

namespace Artemis.Core;

/// <summary>
/// Represents a configuration section that contains a collection of configuration items.
/// </summary>
public class ConfigurationSection : CorePropertyChanged
{
    /// <summary>
    /// Gets or sets the name of the configuration section.
    /// </summary>
    public required string Name
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    /// Gets the collection of configuration items in this section.
    /// </summary>
    public ObservableCollection<IConfigurationItem> Items { get; } = [];
}