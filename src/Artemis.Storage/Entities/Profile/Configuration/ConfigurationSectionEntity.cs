using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Configuration;

public class ConfigurationSectionEntity
{
    public string Name { get; set; } = string.Empty;
    public List<IConfigurationItemEntity> Items { get; set; } = [];
}
