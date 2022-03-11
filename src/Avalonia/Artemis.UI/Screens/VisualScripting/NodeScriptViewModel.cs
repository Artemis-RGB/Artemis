using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptViewModel : ActivatableViewModelBase
{
    private readonly INodeService _nodeService;
    private readonly INodeVmFactory _nodeVmFactory;

    public NodeScriptViewModel(NodeScript nodeScript, INodeVmFactory nodeVmFactory, INodeService nodeService)
    {
        _nodeVmFactory = nodeVmFactory;
        _nodeService = nodeService;

        NodeScript = nodeScript;
        NodePickerViewModel = _nodeVmFactory.NodePickerViewModel(nodeScript);

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
            NodeViewModels.Add(_nodeVmFactory.NodeViewModel(nodeScriptNode));
    }

    public NodeScript NodeScript { get; }
    public ObservableCollection<NodeViewModel> NodeViewModels { get; }
    public NodePickerViewModel NodePickerViewModel { get; }


    private void HandleNodeAdded(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModels.Add(_nodeVmFactory.NodeViewModel(eventArgs.Value));
    }

    private void HandleNodeRemoved(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModel? toRemove = NodeViewModels.FirstOrDefault(vm => vm.Node == eventArgs.Value);
        if (toRemove != null)
            NodeViewModels.Remove(toRemove);
    }

    public void ShowNodePicker(Point position)
    {
        NodePickerViewModel.Show(position);
    }

    public void HideNodePicker()
    {
        NodePickerViewModel.Hide();
    }
}