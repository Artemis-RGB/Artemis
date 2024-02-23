using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to delete a node.
/// </summary>
public class DeleteNode : INodeEditorCommand, IDisposable
{
    private readonly NodeConnectionStore _connections;
    private readonly INode _node;
    private readonly INodeScript _nodeScript;
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

        _connections = new NodeConnectionStore(_node);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global - Provided by plugins
        if (_isRemoved && _node is IDisposable disposableNode)
            disposableNode.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => $"Delete '{_node.Name}' node";

    /// <inheritdoc />
    public void Execute()
    {
        _connections.Store();
        _nodeScript.RemoveNode(_node);

        _isRemoved = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _nodeScript.AddNode(_node);
        _connections.Restore();

        _isRemoved = false;
    }
}