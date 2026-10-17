using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Storage.Entities.Profile.Configuration;

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

    #region Storage

    internal static ConfigurationSection Load(ProfileConfiguration profileConfiguration, ConfigurationSectionEntity entity)
    {
        ConfigurationSection section = new(profileConfiguration) {Name = entity.Name};
        foreach (IConfigurationItemEntity itemEntity in entity.Items)
        {
            IConfigurationItem? item = LoadItem(itemEntity);
            if (item != null)
                section.Items.Add(item);
        }

        return section;
    }

    internal ConfigurationSectionEntity Save()
    {
        return new ConfigurationSectionEntity {Name = Name, Items = Items.Select(SaveItem).ToList()};
    }

    private static IConfigurationItem? LoadItem(IConfigurationItemEntity entity)
    {
        ConfigurationInputItem? inputItem = entity switch
        {
            ConfigurationStringItemEntity e => new ConfigurationStringItem {Id = e.Id, Name = e.Name},
            ConfigurationBooleanItemEntity e => new ConfigurationBooleanItem {Id = e.Id, Name = e.Name, TrueText = e.TrueText, FalseText = e.FalseText},
            ConfigurationNumericItemEntity e => new ConfigurationNumericItem {Id = e.Id, Name = e.Name, Minimum = e.Minimum, Maximum = e.Maximum, Slider = e.Slider},
            ConfigurationSKColorItemEntity e => new ConfigurationSKColorItem {Id = e.Id, Name = e.Name},
            ConfigurationColorGradientItemEntity e => new ConfigurationColorGradientItem {Id = e.Id, Name = e.Name},
            _ => null
        };

        if (inputItem != null)
        {
            inputItem.Load((ConfigurationInputItemEntity) entity);
            return inputItem;
        }

        if (entity is ConfigurationTextItemEntity textItemEntity)
            return new ConfigurationTextItem {Id = textItemEntity.Id, Text = textItemEntity.Text};
        return null;
    }

    private static IConfigurationItemEntity SaveItem(IConfigurationItem item)
    {
        if (item is ConfigurationTextItem textItem)
            return new ConfigurationTextItemEntity {Id = textItem.Id, Text = textItem.Text};

        ConfigurationInputItemEntity entity = item switch
        {
            ConfigurationStringItem => new ConfigurationStringItemEntity(),
            ConfigurationBooleanItem booleanItem => new ConfigurationBooleanItemEntity {TrueText = booleanItem.TrueText, FalseText = booleanItem.FalseText},
            ConfigurationNumericItem numericItem => new ConfigurationNumericItemEntity {Minimum = numericItem.Minimum, Maximum = numericItem.Maximum, Slider = numericItem.Slider},
            ConfigurationSKColorItem => new ConfigurationSKColorItemEntity(),
            ConfigurationColorGradientItem => new ConfigurationColorGradientItemEntity(),
            _ => throw new ArtemisCoreException($"Unknown configuration item type: {item.GetType()}")
        };

        ((ConfigurationInputItem) item).Save(entity);
        return entity;
    }

    #endregion
}