using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Nodes
{
    public class NodeScriptEntity
    {
        public NodeScriptEntity()
        {
            Nodes = new List<NodeEntity>();
            Connections = new List<NodeConnectionEntity>();
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<NodeEntity> Nodes { get; set; }
        public List<NodeConnectionEntity> Connections { get; set; }
    }
}