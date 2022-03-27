using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to move a node.
/// </summary>
public class MoveNode : INodeEditorCommand
{
    private readonly INode _node;
    private readonly double _originalX;
    private readonly double _originalY;
    private readonly double _x;
    private readonly double _y;

    /// <summary>
    ///     Creates a new instance of the <see cref="MoveNode" /> class.
    /// </summary>
    /// <param name="node">The node to update.</param>
    /// <param name="x">The new X-position.</param>
    /// <param name="y">The new Y-position.</param>
    public MoveNode(INode node, double x, double y)
    {
        _node = node;
        _x = x;
        _y = y;

        _originalX = node.X;
        _originalY = node.Y;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="MoveNode" /> class.
    /// </summary>
    /// <param name="node">The node to update.</param>
    /// <param name="x">The new X-position.</param>
    /// <param name="y">The new Y-position.</param>
    /// <param name="originalX">The original X-position.</param>
    /// <param name="originalY">The original Y-position.</param>
    public MoveNode(INode node, double x, double y, double originalX, double originalY)
    {
        _node = node;
        _x = x;
        _y = y;

        _originalX = originalX;
        _originalY = originalY;
    }

    /// <inheritdoc />
    public string DisplayName => "Move node";

    /// <inheritdoc />
    public void Execute()
    {
        _node.X = _x;
        _node.Y = _y;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _node.X = _originalX;
        _node.Y = _originalY;
    }
}