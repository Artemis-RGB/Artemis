using System;

namespace Artemis.Storage.Entities.General;

public class ReleaseEntity
{
    public Guid Id { get; set; }
    
    public string Version { get; set; }
    public string ReleaseId { get; set; }
    public ReleaseEntityStatus Status { get; set; }
    public DateTimeOffset? InstalledAt { get; set; }
}

public enum ReleaseEntityStatus
{
    Queued,
    Installed,
    Historical,
    Unknown
}