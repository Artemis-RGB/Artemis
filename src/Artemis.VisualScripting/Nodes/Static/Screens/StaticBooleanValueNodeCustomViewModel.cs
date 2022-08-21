using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticBooleanValueNodeCustomViewModel : CustomNodeViewModel
{
    private readonly StaticBooleanValueNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public StaticBooleanValueNodeCustomViewModel(StaticBooleanValueNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(CurrentValue));
    }

    public int? CurrentValue
    {
        get => _node.Storage ? 1 : 0;
        set => _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<bool>(_node, value == 1));
    }
}