using System;

namespace Artemis.Storage.Entities.General;

public class ScriptConfigurationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string ScriptingProviderId { get; set; } = string.Empty;
    public string? ScriptContent { get; set; }
}