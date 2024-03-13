using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Entities.Profile;

[Index(nameof(Name), IsUnique = true)]
public class ProfileCategoryEntity
{
    public Guid Id { get; set; }

    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
    public bool IsCollapsed { get; set; }
    public bool IsSuspended { get; set; }
    public int Order { get; set; }

    public List<ProfileContainerEntity> ProfileConfigurations { get; set; } = new();
}