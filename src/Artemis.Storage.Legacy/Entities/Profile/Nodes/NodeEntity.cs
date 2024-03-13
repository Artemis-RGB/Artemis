namespace Artemis.Storage.Legacy.Entities.Profile.Nodes;

internal class NodeEntity
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
    public string Type { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsExitNode { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string Storage { get; set; } = string.Empty;

    public List<NodePinCollectionEntity> PinCollections { get; set; }
}