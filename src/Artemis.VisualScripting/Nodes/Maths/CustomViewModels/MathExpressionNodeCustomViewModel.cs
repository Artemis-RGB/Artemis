using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Validation.Components.Abstractions;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

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

        this.ValidationRule(vm => vm.InputValue, value => _node.IsSyntaxValid(value), value => _node.GetSyntaxErrors(value));

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.Values.PinAdded += x, x => _node.Values.PinAdded -= x)
                .Subscribe(_ =>
                {
                    string? old = InputValue;
                    InputValue = null;
                    InputValue = old;
                })
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => _node.Values.PinRemoved += x, x => _node.Values.PinRemoved -= x)
                .Subscribe(_ =>
                {
                    string? old = InputValue;
                    InputValue = null;
                    InputValue = old;
                })
                .DisposeWith(d);
        });

        NodeModified += (_, _) => InputValue = _node.Storage;
        InputValue = _node.Storage;
    }

    public string? InputValue
    {
        get => _inputValue;
        set => this.RaiseAndSetIfChanged(ref _inputValue, value);
    }

    public void UpdateInputValue()
    {
        // The value could be invalid but that's ok, we still want to save it
        if (_node.Storage != InputValue)
            _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<string>(_node, InputValue));
    }
}