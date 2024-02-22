using System;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to add a node.
/// </summary>
public class AddNode : INodeEditorCommand, IDisposable
{
    private readonly INode _node;
    private readonly INodeScript _nodeScript;
    private bool _isRemoved;

    /// <summary>
    ///     Creates a new instance of the <see cref="AddNode" /> class.
    /// </summary>
    /// <param name="nodeScript">The node script the node belongs to.</param>
    /// <param name="node">The node to delete.</param>
    public AddNode(INodeScript nodeScript, INode node)
    {
        _nodeScript = nodeScript;
        _node = node;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global - Provided by plugins
        if (_isRemoved && _node is IDisposable disposableNode)
            disposableNode.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => $"Add '{_node.Name}' node";

    /// <inheritdoc />
    public void Execute()
    {
        if (!_nodeScript.Nodes.Contains(_node))
            _nodeScript.AddNode(_node);
        _isRemoved = false;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _nodeScript.RemoveNode(_node);
        _isRemoved = true;
    }
}