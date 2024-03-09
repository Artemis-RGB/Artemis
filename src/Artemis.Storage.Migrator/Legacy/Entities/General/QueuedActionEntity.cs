namespace Artemis.Storage.Migrator.Legacy.Entities.General;

public class QueuedActionEntity
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