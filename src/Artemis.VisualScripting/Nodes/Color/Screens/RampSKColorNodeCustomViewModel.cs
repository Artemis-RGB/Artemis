using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public class RampSKColorNodeCustomViewModel : CustomNodeViewModel
{
    private readonly RampSKColorNode _node;
    private readonly INodeEditorService _nodeEditorService;

    /// <inheritdoc />
    public RampSKColorNodeCustomViewModel(RampSKColorNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        Gradient = _node.Storage ?? new ColorGradient();
    }

    public ColorGradient Gradient { get; }

    public void StoreGradient()
    {
        _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<ColorGradient>(_node, Gradient));
    }
}