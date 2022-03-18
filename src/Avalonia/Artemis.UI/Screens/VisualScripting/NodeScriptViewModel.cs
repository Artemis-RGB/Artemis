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
    private readonly INodeVmFactory _nodeVmFactory;
    private readonly INodeEditorService _nodeEditorService;
    private List<NodeViewModel>? _initialNodeSelection;

    public NodeScriptViewModel(NodeScript nodeScript, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        _nodeEditorService = nodeEditorService;

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
        NodeViewModels = new ObservableCollection<NodeViewModel>();
        foreach (INode nodeScriptNode in NodeScript.Nodes)
            NodeViewModels.Add(_nodeVmFactory.NodeViewModel(this, nodeScriptNode));

        // Observe all outgoing pin connections and create cables for them
        IObservable<IChangeSet<NodeViewModel>> viewModels = NodeViewModels.ToObservableChangeSet();
        PinViewModels = viewModels.TransformMany(vm => vm.OutputPinViewModels)
            .Merge(viewModels.TransformMany(vm => vm.InputPinViewModels))
            .Merge(viewModels
                .TransformMany(vm => vm.OutputPinCollectionViewModels)
                .TransformMany(vm => vm.PinViewModels))
            .Merge(viewModels
                .TransformMany(vm => vm.InputPinCollectionViewModels)
                .TransformMany(vm => vm.PinViewModels))
            .AsObservableList();

        PinViewModels.Connect()
            .Filter(p => p.Pin.Direction == PinDirection.Input && p.Pin.ConnectedTo.Any())
            .Transform(vm => _nodeVmFactory.CableViewModel(this, vm.Pin.ConnectedTo.First(), vm.Pin)) // The first pin is the originating output pin
            .Bind(out ReadOnlyObservableCollection<CableViewModel> cableViewModels)
            .Subscribe();

        CableViewModels = cableViewModels;
    }

    public IObservableList<PinViewModel> PinViewModels { get; }

    public PinViewModel? GetPinViewModel(IPin pin)
    {
        return NodeViewModels
            .SelectMany(n => n.Pins)
            .Concat(NodeViewModels.SelectMany(n => n.InputPinCollectionViewModels.SelectMany(c => c.PinViewModels)))
            .Concat(NodeViewModels.SelectMany(n => n.OutputPinCollectionViewModels.SelectMany(c => c.PinViewModels)))
            .FirstOrDefault(vm => vm.Pin == pin);
    }

    public NodeScript NodeScript { get; }
    public ObservableCollection<NodeViewModel> NodeViewModels { get; }
    public ReadOnlyObservableCollection<CableViewModel> CableViewModels { get; }
    public NodePickerViewModel NodePickerViewModel { get; }
    public NodeEditorHistory History { get; }

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

    private void HandleNodeAdded(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModels.Add(_nodeVmFactory.NodeViewModel(this, eventArgs.Value));
    }

    private void HandleNodeRemoved(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModel? toRemove = NodeViewModels.FirstOrDefault(vm => vm.Node == eventArgs.Value);
        if (toRemove != null)
            NodeViewModels.Remove(toRemove);
    }
}