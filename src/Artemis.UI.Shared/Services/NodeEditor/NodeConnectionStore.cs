using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <summary>
///     Represents a class that can store and restore a node's connections
/// </summary>
public class NodeConnectionStore
{
    private readonly Dictionary<IPin, List<IPin>> _pinConnections = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeConnectionStore" /> class.
    /// </summary>
    /// <param name="node">The node whose connections to store</param>
    public NodeConnectionStore(INode node)
    {
        Node = node;
    }

    /// <summary>
    ///     Gets the node this instance will store connections for.
    /// </summary>
    public INode Node { get; }

    /// <summary>
    ///     Stores and clears the current connections of the node
    /// </summary>
    public void Store()
    {
        _pinConnections.Clear();
        foreach (IPin nodePin in Node.Pins)
        {
            _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));
            nodePin.DisconnectAll();
        }

        foreach (IPinCollection nodePinCollection in Node.PinCollections)
        {
            foreach (IPin nodePin in nodePinCollection)
            {
                _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));
                nodePin.DisconnectAll();
            }
        }
    }

    /// <summary>
    ///     Restores the connections of the node as they were during the last <see cref="Store" /> call.
    /// </summary>
    public void Restore()
    {
        foreach (IPin nodePin in Node.Pins)
        {
            if (_pinConnections.TryGetValue(nodePin, out List<IPin>? connections))
                foreach (IPin connection in connections)
                    nodePin.ConnectTo(connection);
        }

        foreach (IPinCollection nodePinCollection in Node.PinCollections)
        {
            foreach (IPin nodePin in nodePinCollection)
            {
                if (_pinConnections.TryGetValue(nodePin, out List<IPin>? connections))
                    foreach (IPin connection in connections)
                        nodePin.ConnectTo(connection);
            }
        }

        _pinConnections.Clear();
    }
}