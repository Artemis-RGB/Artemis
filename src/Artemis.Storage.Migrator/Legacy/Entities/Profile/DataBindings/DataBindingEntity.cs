using Artemis.Storage.Migrator.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile.DataBindings;

public class DataBindingEntity
{
    public bool IsEnabled { get; set; }
    public NodeScriptEntity? NodeScript { get; set; }
}