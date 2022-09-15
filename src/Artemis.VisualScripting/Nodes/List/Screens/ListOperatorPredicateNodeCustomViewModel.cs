using System.Reactive;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.List.Screens;

public class ListOperatorPredicateNodeCustomViewModel : CustomNodeViewModel
{
    private readonly ListOperatorPredicateNode _node;
    private readonly IWindowService _windowService;
    private ListOperator _operator;

    public ListOperatorPredicateNodeCustomViewModel(ListOperatorPredicateNode node, INodeScript script, IWindowService windowService) : base(node, script)
    {
        _node = node;
        _windowService = windowService;

        OpenEditor = ReactiveCommand.CreateFromTask(ExecuteOpenEditor);
    }

    public ReactiveCommand<Unit, Unit> OpenEditor { get; }

    public ListOperator Operator
    {
        get => _operator;
        set => this.RaiseAndSetIfChanged(ref _operator, value);
    }

    private async Task ExecuteOpenEditor()
    {
        if (_node.Script == null)
            return;

        await _windowService.ShowDialogAsync<NodeScriptWindowViewModelBase, bool>(("nodeScript", _node.Script));
        _node.Script.Save();

        _node.Storage ??= new ListOperatorEntity();
        _node.Storage.Script = _node.Script.Entity;
    }
}