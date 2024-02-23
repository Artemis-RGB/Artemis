using System;

namespace Artemis.Storage.Entities.Profile.Nodes;

public class NodeConnectionEntity
{
    public NodeConnectionEntity()
    {
    }

    public NodeConnectionEntity(NodeConnectionEntity nodeConnectionEntity)
    {
        SourceType = nodeConnectionEntity.SourceType;
        SourceNode = nodeConnectionEntity.SourceNode;
        TargetNode = nodeConnectionEntity.TargetNode;
        SourcePinCollectionId = nodeConnectionEntity.SourcePinCollectionId;
        SourcePinId = nodeConnectionEntity.SourcePinId;
        TargetType = nodeConnectionEntity.TargetType;
        TargetPinCollectionId = nodeConnectionEntity.TargetPinCollectionId;
        TargetPinId = nodeConnectionEntity.TargetPinId;
    }

    public string SourceType { get; set; } = string.Empty;
    public Guid SourceNode { get; set; }
    public Guid TargetNode { get; set; }
    public int SourcePinCollectionId { get; set; }
    public int SourcePinId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int TargetPinCollectionId { get; set; }
    public int TargetPinId { get; set; }
}