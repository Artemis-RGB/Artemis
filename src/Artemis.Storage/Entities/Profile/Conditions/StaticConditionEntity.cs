using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Conditions;

public class StaticConditionEntity : IConditionEntity
{
    public int PlayMode { get; set; }
    public int StopMode { get; set; }
    public NodeScriptEntity Script { get; set; }
}