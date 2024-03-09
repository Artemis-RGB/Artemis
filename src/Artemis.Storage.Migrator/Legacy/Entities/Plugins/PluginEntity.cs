namespace Artemis.Storage.Migrator.Legacy.Entities.Plugins;

/// <summary>
///     Represents the configuration of a plugin, each plugin has one configuration
/// </summary>
public class PluginEntity
{
    public PluginEntity()
    {
        Features = new List<PluginFeatureEntity>();
    }

    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }

    public List<PluginFeatureEntity> Features { get; set; }

    public Artemis.Storage.Entities.Plugins.PluginEntity Migrate()
    {
        return new Artemis.Storage.Entities.Plugins.PluginEntity()
        {
            Id = Id,
            IsEnabled = IsEnabled,
            Features = Features.Select(f => f.Migrate()).ToList()
        };
    }
}

/// <summary>
///     Represents the configuration of a plugin feature, each feature has one configuration
/// </summary>
public class PluginFeatureEntity
{
    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    public Artemis.Storage.Entities.Plugins.PluginFeatureEntity Migrate()
    {
        return new Artemis.Storage.Entities.Plugins.PluginFeatureEntity()
        {
            Type = Type,
            IsEnabled = IsEnabled
        };
    }
}