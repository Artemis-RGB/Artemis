using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.VisualScripting.Nodes.External.Commands;

public class UpdateLayerPropertyNodeSelectedProfileElement : INodeEditorCommand
{
    private readonly NodeConnectionStore _connections;
    private readonly LayerPropertyNode _node;
    private readonly ILayerProperty? _oldLayerProperty;
    private readonly RenderProfileElement? _oldValue;

    private readonly RenderProfileElement? _value;

    public UpdateLayerPropertyNodeSelectedProfileElement(LayerPropertyNode node, RenderProfileElement? value)
    {
        _node = node;
        _connections = new NodeConnectionStore(_node);

        _value = value;
        _oldValue = node.ProfileElement;
        _oldLayerProperty = node.LayerProperty;
    }

    /// <inheritdoc />
    public string DisplayName => "Update node profile element";

    /// <inheritdoc />
    public void Execute()
    {
        // Store connections as they currently are
        _connections.Store();

        // Update the selected profile element
        _node.ChangeProfileElement(_value);
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Can't undo it if that profile element is now gone :\
        if (_oldValue != null && _oldValue.Disposed)
            return;

        // Restore the previous profile element
        _node.ChangeProfileElement(_oldValue);

        // Restore the previous layer property
        _node.ChangeLayerProperty(_oldLayerProperty);

        // Restore connections
        _connections.Restore();
    }
}