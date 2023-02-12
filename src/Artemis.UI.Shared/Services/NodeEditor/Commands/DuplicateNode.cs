using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to duplicate a node.
/// </summary>
public class DuplicateNode : INodeEditorCommand, IDisposable
{
    private readonly bool _copyIncomingConnections;
    private readonly INode _node;
    private readonly INodeScript _nodeScript;
    private readonly INodeService _nodeService;
    private NodeConnectionStore? _connections;
    private INode? _copy;

    private bool _executed;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteNode" /> class.
    /// </summary>
    /// <param name="nodeScript">The node script the node belongs to.</param>
    /// <param name="node">The node to delete.</param>
    /// <param name="copyIncomingConnections">A boolean indicating whether incoming connections should be copied.</param>
    /// <param name="nodeService">The service to use to duplicate the node.</param>
    public DuplicateNode(INodeScript nodeScript, INode node, bool copyIncomingConnections, INodeService nodeService)
    {
        _nodeScript = nodeScript;
        _node = node;
        _copyIncomingConnections = copyIncomingConnections;
        _nodeService = nodeService;
    }

    private INode CreateCopy()
    {
        NodeData? nodeData = _nodeService.AvailableNodes.FirstOrDefault(d => d.Type == _node.GetType());
        if (nodeData == null)
            throw new ArtemisSharedUIException($"Can't create a copy of node of type {_node.GetType()} because there is no matching node data.");

        // Create the node
        INode node = nodeData.CreateNode(_nodeScript, null);
        // Move it slightly
        node.X = _node.X + 20;
        node.Y = _node.Y + 20;

        // Add pins to collections
        for (int i = 0; i < _node.PinCollections.Count; i++)
        {
            IPinCollection sourceCollection = _node.PinCollections.ElementAt(i);
            IPinCollection? targetCollection = node.PinCollections.ElementAtOrDefault(i);

            if (targetCollection == null)
                continue;
            while (targetCollection.Count() < sourceCollection.Count())
                targetCollection.Add(targetCollection.CreatePin());
        }

        // Copy the storage
        if (_node is Node sourceNode && node is Node targetNode)
            targetNode.DeserializeStorage(sourceNode.SerializeStorage());

        // Connect input pins
        if (_copyIncomingConnections)
        {
            List<IPin> sourceInputPins = _node.Pins.Concat(_node.PinCollections.SelectMany(c => c)).Where(p => p.Direction == PinDirection.Input).ToList();
            List<IPin> targetInputPins = node.Pins.Concat(node.PinCollections.SelectMany(c => c)).Where(p => p.Direction == PinDirection.Input).ToList();
            for (int i = 0; i < sourceInputPins.Count; i++)
            {
                IPin source = sourceInputPins.ElementAt(i);
                IPin? target = targetInputPins.ElementAtOrDefault(i);

                if (target == null)
                    continue;
                foreach (IPin pin in source.ConnectedTo)
                    target.ConnectTo(pin);
            }
        }

        _connections = new NodeConnectionStore(node);
        return node;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_executed && _copy is IDisposable disposableNode)
            disposableNode.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => $"Duplicate '{_node.Name}' node";

    /// <inheritdoc />
    public void Execute()
    {
        if (_copy == null)
        {
            _copy = CreateCopy();
            _nodeScript.AddNode(_copy);
        }
        else if (_connections != null)
        {
            _nodeScript.AddNode(_copy);
            _connections.Restore();
        }

        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_copy != null && _connections != null)
        {
            _connections.Store();
            _nodeScript.RemoveNode(_copy);
        }

        _executed = false;
    }
}