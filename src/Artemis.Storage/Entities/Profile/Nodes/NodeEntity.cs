using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Nodes
{
    public class NodeEntity
    {
        public NodeEntity()
        {
            PinCollections = new List<NodePinCollectionEntity>();
        }

        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid PluginId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsExitNode { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Storage { get; set; }

        public List<NodePinCollectionEntity> PinCollections { get; set; }
    }
}