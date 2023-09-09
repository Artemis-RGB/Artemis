using System;

namespace Artemis.Storage.Entities.Workshop;

public class EntryEntity
{
    public Guid Id { get; set; }
    
    public long EntryId { get; set; }
    public int EntryType { get; set; }
    
    public string Author { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; }
    public DateTimeOffset InstalledAt { get; set; }
    
    public string LocalReference { get; set; }
}