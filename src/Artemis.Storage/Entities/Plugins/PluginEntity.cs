using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Entities.Plugins;

/// <summary>
///     Represents the configuration of a plugin, each plugin has one configuration
/// </summary>
[Index(nameof(PluginGuid), IsUnique = true)]
public class PluginEntity
{
    public PluginEntity()
    {
        Features = new List<PluginFeatureEntity>();
    }

    public Guid Id { get; set; }
    public Guid PluginGuid { get; set; }
    public bool IsEnabled { get; set; }

    public List<PluginFeatureEntity> Features { get; set; }
}

/// <summary>
///     Represents the configuration of a plugin feature, each feature has one configuration
/// </summary>
public class PluginFeatureEntity
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}