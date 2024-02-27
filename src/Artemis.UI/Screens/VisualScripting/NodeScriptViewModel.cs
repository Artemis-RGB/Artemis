using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Models;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using Avalonia.Input;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public partial class NodeScriptViewModel : ActivatableViewModelBase
{
    public const string CLIPBOARD_DATA_FORMAT = "Artemis.Nodes";
    
    private readonly INodeEditorService _nodeEditorService;
    private readonly INodeService _nodeService;
    private readonly SourceList<NodeViewModel> _nodeViewModels;
    private readonly INodeVmFactory _nodeVmFactory;
    private readonly Subject<Point> _requestedPickerPositionSubject;
    private List<NodeViewModel>? _initialNodeSelection;
    [Notify] private DragCableViewModel? _dragViewModel;
    [Notify] private Matrix _panMatrix;
    [Notify] private Point _pastePosition;

    public NodeScriptViewModel(NodeScript nodeScript, bool isPreview, INodeVmFactory nodeVmFactory, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        _nodeService = nodeService;
        _nodeEditorService = nodeEditorService;
        _nodeViewModels = new SourceList<NodeViewModel>();
        _requestedPickerPositionSubject = new Subject<Point>();

        NodeScript = nodeScript;
        IsPreview = isPreview;
        NodePickerViewModel = _nodeVmFactory.NodePickerViewModel(nodeScript);
        History = nodeEditorService.GetHistory(NodeScript);
        PickerPositionSubject = _requestedPickerPositionSubject.AsObservable();

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<INode>>(x => NodeScript.NodeAdded += x, x => NodeScript.NodeAdded -= x)
                .Subscribe(e => _nodeViewModels.Add(_nodeVmFactory.NodeViewModel(this, e.EventArgs.Value)))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<INode>>(x => NodeScript.NodeRemoved += x, x => NodeScript.NodeRemoved -= x)
                .Subscribe(e => _nodeViewModels.RemoveMany(_nodeViewModels.Items.Where(n => n.Node == e.EventArgs.Value)))
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

        NodeViewModels.ToObservableChangeSet()
            .TransformMany(vm => vm.PinViewModels)
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

        ClearSelection = ReactiveCommand.Create(ExecuteClearSelection);
        DeleteSelected = ReactiveCommand.Create(ExecuteDeleteSelected);
        DuplicateSelected = ReactiveCommand.Create(ExecuteDuplicateSelected);
        CopySelected = ReactiveCommand.CreateFromTask(ExecuteCopySelected);
        PasteSelected = ReactiveCommand.CreateFromTask(ExecutePasteSelected);
    }

    public NodeScript NodeScript { get; }
    public ReadOnlyObservableCollection<NodeViewModel> NodeViewModels { get; }
    public ReadOnlyObservableCollection<PinViewModel> PinViewModels { get; }
    public ReadOnlyObservableCollection<CableViewModel> CableViewModels { get; }
    public NodePickerViewModel NodePickerViewModel { get; }
    public NodeEditorHistory History { get; }
    public IObservable<Point> PickerPositionSubject { get; }

    public bool IsPreview { get; }

    public ReactiveCommand<Unit, Unit> ClearSelection { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelected { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSelected { get; }
    public ReactiveCommand<Unit, Unit> CopySelected { get; }
    public ReactiveCommand<Unit, Unit> PasteSelected { get; }
    
    public void DeleteSelectedNodes()
    {
        List<NodeViewModel> toRemove = NodeViewModels.Where(vm => vm.IsSelected && !vm.Node.IsDefaultNode && !vm.Node.IsExitNode).ToList();
        if (!toRemove.Any())
            return;

        using (_nodeEditorService.CreateCommandScope(NodeScript, "Delete nodes"))
        {
            foreach (NodeViewModel node in toRemove)
                _nodeEditorService.ExecuteCommand(NodeScript, new DeleteNode(NodeScript, node.Node));
        }
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
        if (targetPinVmModel == null)
            return true;
        if (!targetPinVmModel.IsCompatibleWith(sourcePinViewModel))
            return false;

        return sourcePinViewModel.Pin.Direction == PinDirection.Output
            ? !targetPinVmModel.Pin.Node.IsInLoop(sourcePinViewModel.Pin.Node)
            : !sourcePinViewModel.Pin.Node.IsInLoop(targetPinVmModel.Pin.Node);
    }

    public void FinishPinDrag(PinViewModel sourcePinViewModel, PinViewModel? targetPinVmModel, Point position)
    {
        if (DragViewModel == null)
            return;

        DragViewModel = null;

        // If dropped on top of a compatible pin, connect to it
        if (targetPinVmModel != null && targetPinVmModel.IsCompatibleWith(sourcePinViewModel))
        {
            if (sourcePinViewModel.Pin.Direction == PinDirection.Output && !targetPinVmModel.Pin.Node.IsInLoop(sourcePinViewModel.Pin.Node))
                _nodeEditorService.ExecuteCommand(NodeScript, new ConnectPins(sourcePinViewModel.Pin, targetPinVmModel.Pin));
            else if (sourcePinViewModel.Pin.Direction == PinDirection.Input && !sourcePinViewModel.Pin.Node.IsInLoop(targetPinVmModel.Pin.Node))
                _nodeEditorService.ExecuteCommand(NodeScript, new ConnectPins(sourcePinViewModel.Pin, targetPinVmModel.Pin));
        }
        // If not dropped on a pin allow the user to create a new node
        else if (targetPinVmModel == null)
        {
            // If there is only one, spawn that straight away
            List<NodeData> singleCompatibleNode = _nodeService.AvailableNodes.Where(n => n.IsCompatibleWithPin(sourcePinViewModel.Pin)).ToList();
            if (singleCompatibleNode.Count == 1)
            {
                // Borrow the node picker to spawn the node in, even if it's never shown
                NodePickerViewModel.TargetPin = sourcePinViewModel.Pin;
                NodePickerViewModel.CreateNode(singleCompatibleNode.First());
            }
            // Otherwise show the user the picker by requesting it at the drop position
            else
            {
                _requestedPickerPositionSubject.OnNext(position);
                NodePickerViewModel.TargetPin = sourcePinViewModel.Pin;
            }
        }
    }

    public void RequestAutoFit()
    {
        AutoFitRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? AutoFitRequested;

    private void ExecuteClearSelection()
    {
        ClearNodeSelection();
    }

    private void ExecuteDeleteSelected()
    {
        List<INode> nodes = NodeViewModels.Where(vm => vm.IsSelected).Select(vm => vm.Node).ToList();
        using NodeEditorCommandScope scope = _nodeEditorService.CreateCommandScope(NodeScript, "Delete nodes");
        foreach (INode node in nodes.Where(n => !n.IsDefaultNode && !n.IsExitNode))
            _nodeEditorService.ExecuteCommand(NodeScript, new DeleteNode(NodeScript, node));
    }

    private void ExecuteDuplicateSelected()
    {
        int nodeCount = NodeViewModels.Count;
        List<INode> nodes = NodeViewModels.Where(vm => vm.IsSelected).Select(vm => vm.Node).ToList();
        using NodeEditorCommandScope scope = _nodeEditorService.CreateCommandScope(NodeScript, "Duplicate nodes");
        foreach (INode node in nodes.Where(n => !n.IsDefaultNode && !n.IsExitNode))
            _nodeEditorService.ExecuteCommand(NodeScript, new DuplicateNode(NodeScript, node, false, _nodeService));

        // Select only the new nodes
        for (int index = 0; index < NodeViewModels.Count; index++)
        {
            NodeViewModel nodeViewModel = NodeViewModels[index];
            nodeViewModel.IsSelected = index >= nodeCount;
        }
    }

    private async Task ExecuteCopySelected()
    {
        List<INode> nodes = NodeViewModels.Where(vm => vm.IsSelected).Select(vm => vm.Node).Where(n => !n.IsDefaultNode && !n.IsExitNode).ToList();
        DataObject dataObject = new();
        string copy = CoreJson.Serialize(new NodesClipboardModel(NodeScript, nodes));
        dataObject.Set(CLIPBOARD_DATA_FORMAT, copy);
        await Shared.UI.Clipboard.SetDataObjectAsync(dataObject);   
    }

    private async Task ExecutePasteSelected()
    {
        NodesClipboardModel? nodesClipboardModel = await Shared.UI.Clipboard.GetJsonAsync<NodesClipboardModel>(CLIPBOARD_DATA_FORMAT);
        if (nodesClipboardModel == null)
            return;
        
        List<INode> nodes = nodesClipboardModel.Paste(NodeScript, PastePosition.X, PastePosition.Y);
        
        using NodeEditorCommandScope scope = _nodeEditorService.CreateCommandScope(NodeScript, "Paste nodes");
        foreach (INode node in nodes)
            _nodeEditorService.ExecuteCommand(NodeScript, new AddNode(NodeScript, node));

        // Select only the new nodes
        foreach (NodeViewModel nodeViewModel in NodeViewModels)
            nodeViewModel.IsSelected = nodes.Contains(nodeViewModel.Node);
    }
}