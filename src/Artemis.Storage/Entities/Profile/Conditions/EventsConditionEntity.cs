using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class EventsConditionEntity : IConditionEntity
    {
        public int EventOverlapMode { get; set; }
        public List<EventConditionEntity> Events { get; set; } = new();
    }

    public class EventConditionEntity
    {
        public DataModelPathEntity EventPath { get; set; }
        public NodeScriptEntity Script { get; set; }
    }
}