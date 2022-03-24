using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using Avalonia.Controls.Mixins;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptViewModel : ActivatableViewModelBase
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly SourceList<NodeViewModel> _nodeViewModels;
    private readonly INodeVmFactory _nodeVmFactory;
    private DragCableViewModel? _dragViewModel;
    private List<NodeViewModel>? _initialNodeSelection;

    public NodeScriptViewModel(NodeScript nodeScript, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        _nodeEditorService = nodeEditorService;
        _nodeViewModels = new SourceList<NodeViewModel>();

        NodeScript = nodeScript;
        NodePickerViewModel = _nodeVmFactory.NodePickerViewModel(nodeScript);
        History = nodeEditorService.GetHistory(NodeScript);

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<INode>>(x => NodeScript.NodeAdded += x, x => NodeScript.NodeAdded -= x)
                .Subscribe(e => HandleNodeAdded(e.EventArgs))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<INode>>(x => NodeScript.NodeRemoved += x, x => NodeScript.NodeRemoved -= x)
                .Subscribe(e => HandleNodeRemoved(e.EventArgs))
                .DisposeWith(d);
        });

        // Create VMs for all nodes
        _nodeViewModels.Connect().Bind(out ReadOnlyObservableCollection<NodeViewModel> nodeViewModels).Subscribe();
        _nodeViewModels.Edit(l =>
        {
            foreach (INode nodeScriptNode in NodeScript.Nodes)
                l.Add(_nodeVmFactory.NodeViewModel(this, nodeScriptNode));
        });
        NodeViewModels = nodeViewModels;

        NodeViewModels.ToObservableChangeSet().TransformMany(vm => vm.PinViewModels)
            .Bind(out ReadOnlyObservableCollection<PinViewModel> pinViewModels)
            .Subscribe();
        PinViewModels = pinViewModels;

        // Observe all outgoing pin connections and create cables for them
        PinViewModels.ToObservableChangeSet()
            .Filter(p => p.Pin.Direction == PinDirection.Output)
            .TransformMany(p => p.Connections)
            .Filter(p => p.ConnectedTo.Any())
            .Transform(pin => _nodeVmFactory.CableViewModel(this, pin.ConnectedTo.First(), pin))
            .Bind(out ReadOnlyObservableCollection<CableViewModel> cableViewModels)
            .Subscribe();
        CableViewModels = cableViewModels;
    }

    public NodeScript NodeScript { get; }
    public ReadOnlyObservableCollection<NodeViewModel> NodeViewModels { get; }
    public ReadOnlyObservableCollection<PinViewModel> PinViewModels { get; }
    public ReadOnlyObservableCollection<CableViewModel> CableViewModels { get; }
    public NodePickerViewModel NodePickerViewModel { get; }
    public NodeEditorHistory History { get; }

    public DragCableViewModel? DragViewModel
    {
        get => _dragViewModel;
        set => RaiseAndSetIfChanged(ref _dragViewModel, value);
    }

    public void UpdateNodeSelection(List<NodeViewModel> nodes, bool expand, bool invert)
    {
        _initialNodeSelection ??= NodeViewModels.Where(vm => vm.IsSelected).ToList();

        if (expand)
        {
            foreach (NodeViewModel nodeViewModel in nodes)
                nodeViewModel.IsSelected = true;
        }
        else if (invert)
        {
            foreach (NodeViewModel nodeViewModel in nodes)
                nodeViewModel.IsSelected = !_initialNodeSelection.Contains(nodeViewModel);
        }
        else
        {
            foreach (NodeViewModel nodeViewModel in nodes)
                nodeViewModel.IsSelected = true;
            foreach (NodeViewModel nodeViewModel in NodeViewModels.Except(nodes))
                nodeViewModel.IsSelected = false;
        }
    }

    public void FinishNodeSelection()
    {
        _initialNodeSelection = null;
    }

    public void ClearNodeSelection()
    {
        foreach (NodeViewModel nodeViewModel in NodeViewModels)
            nodeViewModel.IsSelected = false;
    }

    public void StartNodeDrag(Point position)
    {
        foreach (NodeViewModel nodeViewModel in NodeViewModels)
            nodeViewModel.StartDrag(position);
    }

    public void UpdateNodeDrag(Point position)
    {
        foreach (NodeViewModel nodeViewModel in NodeViewModels)
            nodeViewModel.UpdateDrag(position);
    }

    public void FinishNodeDrag()
    {
        List<MoveNode> commands = NodeViewModels.Select(n => n.FinishDrag()).Where(c => c != null).Cast<MoveNode>().ToList();

        if (!commands.Any())
            return;

        if (commands.Count == 1)
            _nodeEditorService.ExecuteCommand(NodeScript, commands.First());

        using NodeEditorCommandScope scope = _nodeEditorService.CreateCommandScope(NodeScript, $"Move {commands.Count} nodes");
        foreach (MoveNode moveNode in commands)
            _nodeEditorService.ExecuteCommand(NodeScript, moveNode);
    }

    public bool UpdatePinDrag(PinViewModel sourcePinViewModel, PinViewModel? targetPinVmModel, Point position)
    {
        if (DragViewModel?.PinViewModel != sourcePinViewModel)
            DragViewModel = new DragCableViewModel(sourcePinViewModel);

        DragViewModel.DragPoint = position;

        return targetPinVmModel == null || targetPinVmModel.IsCompatibleWith(sourcePinViewModel);
    }

    public void FinishPinDrag(PinViewModel sourcePinViewModel, PinViewModel? targetPinVmModel)
    {
        if (DragViewModel == null)
            return;

        DragViewModel = null;

        // If dropped on top of a compatible pin, connect to it
        if (targetPinVmModel != null && targetPinVmModel.IsCompatibleWith(sourcePinViewModel))
            _nodeEditorService.ExecuteCommand(NodeScript, new ConnectPins(sourcePinViewModel.Pin, targetPinVmModel.Pin));
    }

    private void HandleNodeAdded(SingleValueEventArgs<INode> eventArgs)
    {
        _nodeViewModels.Add(_nodeVmFactory.NodeViewModel(this, eventArgs.Value));
    }

    private void HandleNodeRemoved(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModel? toRemove = NodeViewModels.FirstOrDefault(vm => ReferenceEquals(vm.Node, eventArgs.Value));
        if (toRemove != null)
            _nodeViewModels.Remove(toRemove);
    }
}