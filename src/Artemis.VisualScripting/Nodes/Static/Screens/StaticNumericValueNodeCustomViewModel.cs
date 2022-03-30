using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticNumericValueNodeCustomViewModel : CustomNodeViewModel
{
    private readonly StaticNumericValueNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public StaticNumericValueNodeCustomViewModel(StaticNumericValueNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(CurrentValue));
    }

    public Numeric? CurrentValue
    {
        get => _node.Storage;
        set => _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<Numeric>(_node, value ?? new Numeric()));
    }
}