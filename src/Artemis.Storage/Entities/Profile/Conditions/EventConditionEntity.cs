using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class EventConditionEntity : IConditionEntity
    {
        public int TriggerMode { get; set; }
        public int OverlapMode { get; set; }
        public int ToggleOffMode { get; set; }
        public DataModelPathEntity EventPath { get; set; }
        public NodeScriptEntity Script { get; set; }
    }
}