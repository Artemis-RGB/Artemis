using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.VisualScripting.Nodes.External.Commands;

public class UpdateLayerPropertyNodeSelectedLayerProperty : INodeEditorCommand
{
    private readonly LayerPropertyNode _node;
    private readonly ILayerProperty? _value;
    private readonly ILayerProperty? _oldValue;
    private readonly NodeConnectionStore _connections;

    public UpdateLayerPropertyNodeSelectedLayerProperty(LayerPropertyNode node, ILayerProperty? value)
    {
        _node = node;
        _connections = new NodeConnectionStore(_node);

        _value = value;
        _oldValue = _node.LayerProperty;
    }


    /// <inheritdoc />
    public string DisplayName => "Update node layer property";

    /// <inheritdoc />
    public void Execute()
    {
        // Store connections as they currently are
        _connections.Store();

        // Update the selected profile element
        _node.ChangeLayerProperty(_value);
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Restore the previous layer property
        _node.ChangeLayerProperty(_oldValue);

        // Restore connections
        _connections.Restore();
    }
}