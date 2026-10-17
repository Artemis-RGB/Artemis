using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Configuration;

public abstract class ConfigurationInputItemEntity : IConfigurationItemEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    ///     The JSON-serialized default value as set by the author of the profile
    /// </summary>
    public string? DefaultValue { get; set; }

    public List<ConfigurationDropdownValueEntity> DropdownValues { get; set; } = [];
}

public class ConfigurationDropdownValueEntity
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     The JSON-serialized value of the dropdown option
    /// </summary>
    public string? Value { get; set; }
}

public class ConfigurationStringItemEntity : ConfigurationInputItemEntity;

public class ConfigurationSKColorItemEntity : ConfigurationInputItemEntity;

public class ConfigurationColorGradientItemEntity : ConfigurationInputItemEntity;

public class ConfigurationBooleanItemEntity : ConfigurationInputItemEntity
{
    public string TrueText { get; set; } = string.Empty;
    public string FalseText { get; set; } = string.Empty;
}

public class ConfigurationNumericItemEntity : ConfigurationInputItemEntity
{
    public float? Minimum { get; set; }
    public float? Maximum { get; set; }
    public bool Slider { get; set; }
}
