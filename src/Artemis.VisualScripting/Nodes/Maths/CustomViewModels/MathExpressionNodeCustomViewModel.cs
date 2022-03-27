using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.VisualScripting.Nodes.Maths.CustomViewModels;

public class MathExpressionNodeCustomViewModel : CustomNodeViewModel
{
    private readonly MathExpressionNode _node;
    private readonly INodeEditorService _nodeEditorService;
    private string? _inputValue;

    public MathExpressionNodeCustomViewModel(MathExpressionNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => InputValue = _node.Storage;
        this.ValidationRule(vm => vm.InputValue, value => _node.IsSyntaxValid(value), value => _node.GetSyntaxErrors(value));
    }

    public string? InputValue
    {
        get => _inputValue;
        set => this.RaiseAndSetIfChanged(ref _inputValue, value);
    }

    public void UpdateInputValue()
    {
        if (!HasErrors && _node.Storage != InputValue)
            _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<string>(_node, InputValue));
    }
}