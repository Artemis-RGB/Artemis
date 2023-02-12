using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.UI.Models;

public class NodesClipboardModel
{
    public NodesClipboardModel(NodeScript nodeScript, List<INode> nodes)
    {
        nodeScript.Save();

        // Grab all entities belonging to provided nodes
        Nodes = nodeScript.Entity.Nodes.Where(e => nodes.Any(n => n.Id == e.Id)).ToList();
        // Grab all connections between provided nodes
        Connections = nodeScript.Entity.Connections.Where(e => nodes.Any(n => n.Id == e.SourceNode) && nodes.Any(n => n.Id == e.TargetNode)).ToList();
    }

    public NodesClipboardModel()
    {
        Nodes = new List<NodeEntity>();
        Connections = new List<NodeConnectionEntity>();
    }

    public List<NodeEntity> Nodes { get; set; }
    public List<NodeConnectionEntity> Connections { get; set; }

    public List<INode> Paste(NodeScript nodeScript, double x, double y)
    {
        if (!Nodes.Any())
            return new List<INode>();
        
        nodeScript.Save();

        // Copy the entities, not messing with the originals
        List<NodeEntity> nodes = Nodes.Select(n => new NodeEntity(n)).ToList();
        List<NodeConnectionEntity> connections = Connections.Select(c => new NodeConnectionEntity(c)).ToList();

        double xOffset = x - nodes.Min(n => n.X);
        double yOffset = y - nodes.Min(n => n.Y);

        foreach (NodeEntity node in nodes)
        {
            // Give each node a new GUID, updating any connections to it
            Guid newGuid = Guid.NewGuid();
            foreach (NodeConnectionEntity connection in connections)
            {
                if (connection.SourceNode == node.Id)
                    connection.SourceNode = newGuid;
                else if (connection.TargetNode == node.Id)
                    connection.TargetNode = newGuid;

                // Only add the connection if this is the first time we hit it
                if (!nodeScript.Entity.Connections.Contains(connection))
                    nodeScript.Entity.Connections.Add(connection);
            }

            node.Id = newGuid;
            node.X += xOffset;
            node.Y += yOffset;
            nodeScript.Entity.Nodes.Add(node);
        }

        nodeScript.Load();

        // Return the newly created nodes
        return nodeScript.Nodes.Where(n => nodes.Any(e => e.Id == n.Id)).ToList();
    }
}