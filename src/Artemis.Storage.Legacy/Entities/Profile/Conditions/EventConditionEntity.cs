using Artemis.Storage.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Legacy.Entities.Profile.Conditions;

internal class EventConditionEntity : IConditionEntity
{
    public int TriggerMode { get; set; }
    public int OverlapMode { get; set; }
    public int ToggleOffMode { get; set; }
    public DataModelPathEntity? EventPath { get; set; }
    public NodeScriptEntity? Script { get; set; }
}