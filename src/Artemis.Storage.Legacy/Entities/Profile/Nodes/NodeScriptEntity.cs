namespace Artemis.Storage.Legacy.Entities.Profile.Nodes;

internal class NodeScriptEntity
{
    public NodeScriptEntity()
    {
        Nodes = new List<NodeEntity>();
        Connections = new List<NodeConnectionEntity>();
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<NodeEntity> Nodes { get; set; }
    public List<NodeConnectionEntity> Connections { get; set; }
}