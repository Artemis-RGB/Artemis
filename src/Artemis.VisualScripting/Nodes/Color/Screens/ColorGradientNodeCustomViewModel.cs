using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public class ColorGradientNodeCustomViewModel : CustomNodeViewModel
{
    private readonly ColorGradientNode _node;
    private readonly INodeEditorService _nodeEditorService;

    /// <inheritdoc />
    public ColorGradientNodeCustomViewModel(ColorGradientNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;
        
        Gradient = _node.Gradient;
    }

    public ColorGradient Gradient { get; }

    public void StoreGradient()
    {
        _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<ColorGradient>(_node, Gradient));
    }
}