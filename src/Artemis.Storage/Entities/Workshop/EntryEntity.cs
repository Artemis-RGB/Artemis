using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Artemis.Storage.Entities.Workshop;

public class EntryEntity
{
    public Guid Id { get; set; }
    
    public long EntryId { get; set; }
    public int EntryType { get; set; }
    
    public string Author { get; set; }  = string.Empty;
    public string Name { get; set; } = string.Empty;

    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; }

    public Dictionary<string, object> Metadata { get; set; }
}