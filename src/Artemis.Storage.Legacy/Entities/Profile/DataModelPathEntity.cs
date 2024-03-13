namespace Artemis.Storage.Legacy.Entities.Profile;

internal class DataModelPathEntity
{
    public string Path { get; set; } = string.Empty;
    public string? DataModelId { get; set; }
    public string? Type { get; set; }
}