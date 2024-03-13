namespace Artemis.Storage.Legacy.Entities.Profile.Nodes;

internal class NodePinCollectionEntity
{
    public NodePinCollectionEntity()
    {
    }

    public NodePinCollectionEntity(NodePinCollectionEntity nodePinCollectionEntity)
    {
        Id = nodePinCollectionEntity.Id;
        Direction = nodePinCollectionEntity.Direction;
        Amount = nodePinCollectionEntity.Amount;
    }

    public int Id { get; set; }
    public int Direction { set; get; }
    public int Amount { get; set; }
}