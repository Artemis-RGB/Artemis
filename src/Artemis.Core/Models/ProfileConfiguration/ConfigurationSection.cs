using System.Collections.ObjectModel;

namespace Artemis.Core;

/// <summary>
///     Represents a configuration section that contains a collection of configuration items.
/// </summary>
public class ConfigurationSection : CorePropertyChanged
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileConfiguration" /> class.
    /// </summary>
    /// <param name="profileConfiguration">The profile configuration this section belongs to.</param>
    public ConfigurationSection(ProfileConfiguration profileConfiguration)
    {
        ProfileConfiguration = profileConfiguration;
    }

    /// <summary>
    ///     Gets the profile configuration this section belongs to.
    /// </summary>
    public ProfileConfiguration ProfileConfiguration { get; }

    /// <summary>
    ///     Gets or sets the name of the configuration section.
    /// </summary>
    public required string Name
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets the collection of configuration items in this section.
    /// </summary>
    public ObservableCollection<IConfigurationItem> Items { get; } = [];
}