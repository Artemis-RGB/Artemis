using Artemis.Storage.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Legacy.Entities.Profile.DataBindings;

internal class DataBindingEntity
{
    public bool IsEnabled { get; set; }
    public NodeScriptEntity? NodeScript { get; set; }
}