namespace Artemis.Storage.Legacy.Entities.General;

internal class ReleaseEntity
{
    public Guid Id { get; set; }

    public string Version { get; set; } = string.Empty;
    public DateTimeOffset? InstalledAt { get; set; }

    public Storage.Entities.General.ReleaseEntity Migrate()
    {
        return new Storage.Entities.General.ReleaseEntity()
        {
            Id = Id,
            Version = Version,
            InstalledAt = InstalledAt ?? DateTimeOffset.Now
        };
    }
}