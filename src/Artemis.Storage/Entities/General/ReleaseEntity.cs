using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Entities.General;

[Index(nameof(Version), IsUnique = true)]
[Index(nameof(InstalledAt))]
public class ReleaseEntity
{
    public Guid Id { get; set; }

    [MaxLength(64)]
    public string Version { get; set; } = string.Empty;

    public DateTimeOffset? InstalledAt { get; set; }
}