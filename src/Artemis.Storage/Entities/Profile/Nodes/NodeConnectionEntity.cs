using System;

namespace Artemis.Storage.Entities.Profile.Nodes
{
    public class NodeConnectionEntity
    {
        public string SourceType { get; set; }
        public Guid SourceNode { get; set; }
        public Guid TargetNode { get; set; }
        public int SourcePinCollectionId { get; set; }
        public int SourcePinId { get; set; }
        public string TargetType { get; set; }
        public int TargetPinCollectionId { get; set; }
        public int TargetPinId { get; set; }
    }
}