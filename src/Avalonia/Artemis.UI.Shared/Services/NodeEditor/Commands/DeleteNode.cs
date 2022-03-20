using System;
using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to delete a node.
/// </summary>
public class DeleteNode : INodeEditorCommand, IDisposable
{
    private readonly INode _node;
    private readonly INodeScript _nodeScript;
    private readonly Dictionary<IPin, List<IPin>> _pinConnections = new();
    private bool _isRemoved;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteNode" /> class.
    /// </summary>
    /// <param name="nodeScript">The node script the node belongs to.</param>
    /// <param name="node">The node to delete.</param>
    public DeleteNode(INodeScript nodeScript, INode node)
    {
        _nodeScript = nodeScript;
        _node = node;
    }

    private void StoreConnections()
    {
        _pinConnections.Clear();
        foreach (IPin nodePin in _node.Pins)
        {
            _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));
            nodePin.DisconnectAll();
        }

        foreach (IPinCollection nodePinCollection in _node.PinCollections)
        {
            foreach (IPin nodePin in nodePinCollection)
            {
                _pinConnections.Add(nodePin, new List<IPin>(nodePin.ConnectedTo));
                nodePin.DisconnectAll();
            }
        }
    }

    private void RestoreConnections()
    {
        foreach (IPin nodePin in _node.Pins)
        {
            if (_pinConnections.TryGetValue(nodePin, out List<IPin>? connections))
            {
                foreach (IPin connection in connections)
                    nodePin.ConnectTo(connection);
            }
        }

        foreach (IPinCollection nodePinCollection in _node.PinCollections)
        {
            foreach (IPin nodePin in nodePinCollection)
            {
                if (_pinConnections.TryGetValue(nodePin, out List<IPin>? connections))
                {
                    foreach (IPin connection in connections)
                        nodePin.ConnectTo(connection);
                }
            }
        }

        _pinConnections.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isRemoved)
            _nodeScript.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => $"Delete '{_node.Name}' node";

    /// <inheritdoc />
    public void Execute()
    {
        StoreConnections();
        _nodeScript.RemoveNode(_node);

        _isRemoved = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _nodeScript.AddNode(_node);
        RestoreConnections();

        _isRemoved = false;
    }
}