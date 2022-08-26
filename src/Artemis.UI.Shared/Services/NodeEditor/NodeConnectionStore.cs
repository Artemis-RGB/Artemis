using System.Collections.Generic;
using System.Linq;
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
        
        // Iterate to save
        foreach (IPin nodePin in Node.Pins.ToList())
            _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));
        foreach (IPin nodePin in Node.PinCollections.ToList().SelectMany(nodePinCollection => nodePinCollection))
            _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));

        // Iterate to disconnect
        foreach (IPin nodePin in Node.Pins.ToList())
            nodePin.DisconnectAll();
        foreach (IPin nodePin in Node.PinCollections.ToList().SelectMany(nodePinCollection => nodePinCollection))
            nodePin.DisconnectAll();
    }

    /// <summary>
    ///     Restores the connections of the node as they were during the last <see cref="Store" /> call.
    /// </summary>
    public void Restore()
    {
        foreach ((IPin? pin, List<IPin>? connections) in _pinConnections)
        {
            foreach (IPin connection in connections)
                pin.ConnectTo(connection);
        }

        _pinConnections.Clear();
    }
}