using System.ComponentModel.DataAnnotations;

namespace Artemis.WebClient.Workshop.Entities;

public class Release
{
    public long Id { get; set; }

    [MaxLength(64)]
    public string Version { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public long Downloads { get; set; }

    public long DownloadSize { get; set; }

    [MaxLength(32)]
    public string? Md5Hash { get; set; }

    public long EntryId { get; set; }
}