using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class EventConditionEntity : IConditionEntity
    {
        public int EventOverlapMode { get; set; }
        public DataModelPathEntity EventPath { get; set; }
        public NodeScriptEntity Script { get; set; }
    }
}