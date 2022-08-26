using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.List.Screens;

public class ListOperatorNodeCustomViewModel : CustomNodeViewModel
{
    private readonly ListOperatorNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public ListOperatorNodeCustomViewModel(ListOperatorNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(CurrentValue));
    }

    public ListOperator CurrentValue
    {
        get => _node.Storage;
        set => _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<ListOperator>(_node, value));
    }
}