using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptViewModel : ActivatableViewModelBase
{
    private readonly INodeVmFactory _nodeVmFactory;
    private List<NodeViewModel>? _initialNodeSelection;

    public NodeScriptViewModel(NodeScript nodeScript, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;

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

        NodeViewModels = new ObservableCollection<NodeViewModel>();
        foreach (INode nodeScriptNode in NodeScript.Nodes)
            NodeViewModels.Add(_nodeVmFactory.NodeViewModel(this, nodeScriptNode));

        CableViewModels = new ObservableCollection<CableViewModel>();
    }

    public NodeScript NodeScript { get; }
    public ObservableCollection<NodeViewModel> NodeViewModels { get; }
    public ObservableCollection<CableViewModel> CableViewModels { get; }
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
            nodeViewModel.SaveDragOffset(position);
    }

    public void UpdateNodeDrag(Point position)
    {
        foreach (NodeViewModel nodeViewModel in NodeViewModels)
            nodeViewModel.UpdatePosition(position);
    }

    public void FinishNodeDrag()
    {
        // TODO: Command
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