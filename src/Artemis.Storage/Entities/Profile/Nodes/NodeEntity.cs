using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Storage.Entities.Profile.Nodes;

public class NodeEntity
{
    public NodeEntity()
    {
        PinCollections = new List<NodePinCollectionEntity>();
    }

    public NodeEntity(NodeEntity nodeEntity)
    {
        Id = nodeEntity.Id;
        Type = nodeEntity.Type;
        ProviderId = nodeEntity.ProviderId;

        Name = nodeEntity.Name;
        Description = nodeEntity.Description;
        IsExitNode = nodeEntity.IsExitNode;
        X = nodeEntity.X;
        Y = nodeEntity.Y;
        Storage = nodeEntity.Storage;

        PinCollections = nodeEntity.PinCollections.Select(p => new NodePinCollectionEntity(p)).ToList();
    }

    public Guid Id { get; set; }
    public string Type { get; set; }
    public string ProviderId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsExitNode { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string Storage { get; set; }

    public List<NodePinCollectionEntity> PinCollections { get; set; }
}