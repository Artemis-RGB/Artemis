using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Entities.Workshop;

[Index(nameof(EntryId), IsUnique = true)]
public class EntryEntity
{
    public Guid Id { get; set; }

    public long EntryId { get; set; }
    public int EntryType { get; set; }

    public string Author { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public long Downloads { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? LatestReleaseId { get; set; }
    
    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; }
    public bool AutoUpdate { get; set; }
    
    public Dictionary<string, JsonNode>? Metadata { get; set; }
    public List<EntryCategoryEntity>? Categories { get; set; }
}

public record EntryCategoryEntity(string Name, string Icon);