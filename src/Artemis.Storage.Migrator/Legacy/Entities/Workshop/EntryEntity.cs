namespace Artemis.Storage.Migrator.Legacy.Entities.Workshop;

public class EntryEntity
{
    public Guid Id { get; set; }

    public long EntryId { get; set; }
    public int EntryType { get; set; }

    public string Author { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public Storage.Entities.Workshop.EntryEntity Migrate()
    {
        // Create a copy
        return new Storage.Entities.Workshop.EntryEntity()
        {
            Id = Id,
            EntryId = EntryId,
            EntryType = EntryType,
            Author = Author,
            Name = Name,
            ReleaseId = ReleaseId,
            ReleaseVersion = ReleaseVersion,
            InstalledAt = InstalledAt,
            Metadata = Metadata ?? new Dictionary<string, object>()
        };
    }
}