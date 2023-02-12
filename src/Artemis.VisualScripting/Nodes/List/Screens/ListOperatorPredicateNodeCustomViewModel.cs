using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Events;
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
    private bool _canOpenEditor;

    public ListOperatorPredicateNodeCustomViewModel(ListOperatorPredicateNode node, INodeScript script, IWindowService windowService) : base(node, script)
    {
        _node = node;
        _windowService = windowService;

        OpenEditor = ReactiveCommand.CreateFromTask(ExecuteOpenEditor, this.WhenAnyValue(vm => vm.CanOpenEditor));
        CanOpenEditor = node.InputList.ConnectedTo.Any();
        
        this.WhenActivated(d =>
        {
            node.InputList.PinConnected += InputListOnPinConnected;
            node.InputList.PinDisconnected += InputListOnPinDisconnected;

            Disposable.Create(() =>
            {
                node.InputList.PinConnected -= InputListOnPinConnected;
                node.InputList.PinDisconnected -= InputListOnPinDisconnected;
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> OpenEditor { get; }

    private bool CanOpenEditor
    {
        get => _canOpenEditor;
        set => this.RaiseAndSetIfChanged(ref _canOpenEditor, value);
    }

    public ListOperator Operator
    {
        get => _operator;
        set => this.RaiseAndSetIfChanged(ref _operator, value);
    }

    private async Task ExecuteOpenEditor()
    {
        if (_node.Script == null)
            return;

        await _windowService.ShowDialogAsync<NodeScriptWindowViewModelBase, bool>(_node.Script);
        _node.Script.Save();

        _node.Storage ??= new ListOperatorEntity();
        _node.Storage.Script = _node.Script.Entity;
    }

    private void InputListOnPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        CanOpenEditor = false;
    }

    private void InputListOnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        CanOpenEditor = true;
    }
}