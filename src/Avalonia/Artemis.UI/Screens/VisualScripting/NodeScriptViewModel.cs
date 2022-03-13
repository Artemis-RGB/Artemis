using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptViewModel : ActivatableViewModelBase
{
    private readonly INodeService _nodeService;
    private readonly INodeEditorService _nodeEditorService;
    private readonly INodeVmFactory _nodeVmFactory;

    public NodeScriptViewModel(NodeScript nodeScript, INodeVmFactory nodeVmFactory, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        _nodeService = nodeService;
        _nodeEditorService = nodeEditorService;

        NodeScript = nodeScript;
        NodePickerViewModel = _nodeVmFactory.NodePickerViewModel(nodeScript);
        History = _nodeEditorService.GetHistory(NodeScript);

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
            NodeViewModels.Add(_nodeVmFactory.NodeViewModel(NodeScript, nodeScriptNode));
    }

    public NodeScript NodeScript { get; }
    public ObservableCollection<NodeViewModel> NodeViewModels { get; }
    public ObservableCollection<CableViewModel> CableViewModels { get; }
    public NodePickerViewModel NodePickerViewModel { get; }
    public NodeEditorHistory History { get; }

    private void HandleNodeAdded(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModels.Add(_nodeVmFactory.NodeViewModel(NodeScript, eventArgs.Value));
    }

    private void HandleNodeRemoved(SingleValueEventArgs<INode> eventArgs)
    {
        NodeViewModel? toRemove = NodeViewModels.FirstOrDefault(vm => vm.Node == eventArgs.Value);
        if (toRemove != null)
            NodeViewModels.Remove(toRemove);
    }
}