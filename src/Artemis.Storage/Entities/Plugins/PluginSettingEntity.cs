using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Entities.Plugins;

/// <summary>
///     Represents the setting of a plugin, a plugin can have multiple settings
/// </summary>
[Index(nameof(Name), nameof(PluginGuid), IsUnique = true)]
[Index(nameof(PluginGuid))]
public class PluginSettingEntity
{
    public Guid Id { get; set; }
    public Guid PluginGuid { get; set; }

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}