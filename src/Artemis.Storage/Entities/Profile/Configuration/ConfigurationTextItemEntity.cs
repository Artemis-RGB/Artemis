using System;

namespace Artemis.Storage.Entities.Profile.Configuration;

public class ConfigurationTextItemEntity : IConfigurationItemEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
