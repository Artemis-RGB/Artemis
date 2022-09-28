using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to organize a script
/// </summary>
public class OrganizeScript : INodeEditorCommand
{
    private readonly NodeScript _script;
    private readonly List<(INode node, double x, double y)> _originalPositions;

    /// <summary>
    /// Creates a new instance of the <see cref="OrganizeScript"/> class.
    /// </summary>
    /// <param name="script">The script to organize.</param>
    public OrganizeScript(NodeScript script)
    {
        _script = script;
        _originalPositions = script.Nodes.Select(n => (n, n.X, n.Y)).ToList();
    }

    /// <inheritdoc />
    public string DisplayName => "Organize script";

    /// <inheritdoc />
    public void Execute()
    {
        _script.Organize();
    }

    /// <inheritdoc />
    public void Undo()
    {
        foreach ((INode? node, double x, double y) in _originalPositions)
        {
            node.X = x;
            node.Y = y;
        }
    }
}