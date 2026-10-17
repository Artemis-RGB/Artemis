using System;
using System.Text.Json.Serialization;

namespace Artemis.Storage.Entities.Profile.Configuration;

[JsonDerivedType(typeof(ConfigurationTextItemEntity), "Text")]
[JsonDerivedType(typeof(ConfigurationStringItemEntity), "String")]
[JsonDerivedType(typeof(ConfigurationBooleanItemEntity), "Boolean")]
[JsonDerivedType(typeof(ConfigurationNumericItemEntity), "Numeric")]
[JsonDerivedType(typeof(ConfigurationSKColorItemEntity), "SKColor")]
[JsonDerivedType(typeof(ConfigurationColorGradientItemEntity), "ColorGradient")]
public interface IConfigurationItemEntity
{
    Guid Id { get; set; }
}
