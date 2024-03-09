namespace Artemis.Storage.Migrator.Legacy.Entities.Plugins;

/// <summary>
///     Represents the setting of a plugin, a plugin can have multiple settings
/// </summary>
public class PluginSettingEntity
{
    public Guid Id { get; set; }
    public Guid PluginGuid { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Artemis.Storage.Entities.Plugins.PluginSettingEntity Migrate()
    {
        return new Storage.Entities.Plugins.PluginSettingEntity
        {
            Id = Id,
            PluginGuid = PluginGuid,
            Name = Name,
            Value = Value
        };
    }
}