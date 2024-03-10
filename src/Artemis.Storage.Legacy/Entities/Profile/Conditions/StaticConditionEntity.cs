using Artemis.Storage.Legacy.Entities.Profile.Nodes;

namespace Artemis.Storage.Legacy.Entities.Profile.Conditions;

internal class StaticConditionEntity : IConditionEntity
{
    public int PlayMode { get; set; }
    public int StopMode { get; set; }
    public NodeScriptEntity? Script { get; set; }
}