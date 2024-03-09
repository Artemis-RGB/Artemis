using Artemis.Storage.Migrator.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile.Conditions;

public class StaticConditionEntity : IConditionEntity
{
    public int PlayMode { get; set; }
    public int StopMode { get; set; }
    public NodeScriptEntity? Script { get; set; }
}