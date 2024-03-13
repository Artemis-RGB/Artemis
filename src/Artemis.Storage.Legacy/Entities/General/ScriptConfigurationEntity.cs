namespace Artemis.Storage.Legacy.Entities.General;

internal class ScriptConfigurationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string ScriptingProviderId { get; set; } = string.Empty;
    public string? ScriptContent { get; set; }
}