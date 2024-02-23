using System;

namespace Artemis.Storage.Entities.General;

public class ReleaseEntity
{
    public Guid Id { get; set; }

    public string Version { get; set; } = string.Empty;
    public DateTimeOffset? InstalledAt { get; set; }
}