namespace Artemis.Storage.Migrator.Legacy.Entities.General;

public class ScriptConfigurationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string ScriptingProviderId { get; set; } = string.Empty;
    public string? ScriptContent { get; set; }
}