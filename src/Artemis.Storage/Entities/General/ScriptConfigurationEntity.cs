using System;

namespace Artemis.Storage.Entities.General;

public class ScriptConfigurationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string ScriptingProviderId { get; set; }
    public string ScriptContent { get; set; }
}