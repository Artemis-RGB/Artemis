using System.ComponentModel.DataAnnotations;

namespace Artemis.Web.Workshop.Entities;

public class Release
{
    public Guid Id { get; set; }

    [MaxLength(64)]
    public string Version { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public long Downloads { get; set; }

    public long DownloadSize { get; set; }

    [MaxLength(32)]
    public string? Md5Hash { get; set; }

    public Guid EntryId { get; set; }
}