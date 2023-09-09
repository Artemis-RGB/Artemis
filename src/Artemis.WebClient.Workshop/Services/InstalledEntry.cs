using Artemis.Storage.Entities.Workshop;

namespace Artemis.WebClient.Workshop.Services;

public class InstalledEntry
{
    internal InstalledEntry(EntryEntity entity)
    {
        Entity = entity;

        Load();
    }

    public InstalledEntry(IGetEntryById_Entry entry)
    {
        Entity = new EntryEntity();

        EntryId = entry.Id;
        EntryType = entry.EntryType;

        Author = entry.Author;
        Name = entry.Name;
    }

    public long EntryId { get; set; }
    public EntryType EntryType { get; set; }

    public string Author { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; }

    public string? LocalReference { get; set; }
    
    internal EntryEntity Entity { get; }

    internal void Load()
    {
        EntryId = Entity.EntryId;
        EntryType = (EntryType) Entity.EntryType;

        Author = Entity.Author;
        Name = Entity.Name;

        ReleaseId = Entity.ReleaseId;
        ReleaseVersion = Entity.ReleaseVersion;
        InstalledAt = Entity.InstalledAt;

        LocalReference = Entity.LocalReference;
    }

    internal void Save()
    {
        Entity.EntryId = EntryId;
        Entity.EntryType = (int) EntryType;

        Entity.Author = Author;
        Entity.Name = Name;

        Entity.ReleaseId = ReleaseId;
        Entity.ReleaseVersion = ReleaseVersion;
        Entity.InstalledAt = InstalledAt;

        Entity.LocalReference = LocalReference;
    }
}