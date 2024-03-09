using Artemis.Storage.Migrator.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile.Conditions;

public class EventConditionEntity : IConditionEntity
{
    public int TriggerMode { get; set; }
    public int OverlapMode { get; set; }
    public int ToggleOffMode { get; set; }
    public DataModelPathEntity? EventPath { get; set; }
    public NodeScriptEntity? Script { get; set; }
}