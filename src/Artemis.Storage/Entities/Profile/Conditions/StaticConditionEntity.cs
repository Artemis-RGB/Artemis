using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class StaticConditionEntity : IConditionEntity
    {
        public NodeScriptEntity Script { get; set; }
    }
}