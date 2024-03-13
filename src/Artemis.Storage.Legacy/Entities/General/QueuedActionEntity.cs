namespace Artemis.Storage.Legacy.Entities.General;

internal class QueuedActionEntity
{
    public QueuedActionEntity()
    {
        Parameters = new Dictionary<string, object>();
    }

    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Dictionary<string, object> Parameters { get; set; }
}