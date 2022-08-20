using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticStringValueNodeCustomViewModel : CustomNodeViewModel
{
    private readonly StaticStringValueNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public StaticStringValueNodeCustomViewModel(StaticStringValueNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(CurrentValue));
    }

    public string? CurrentValue
    {
        get => _node.Storage;
        set => _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<string>(_node, value));
    }
}