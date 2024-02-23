using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.DataBindings;

public class DataBindingEntity
{
    public bool IsEnabled { get; set; }
    public NodeScriptEntity? NodeScript { get; set; }
}