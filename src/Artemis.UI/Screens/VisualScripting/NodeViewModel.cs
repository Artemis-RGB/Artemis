using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Layout;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeViewModel : ActivatableViewModelBase
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly IWindowService _windowService;

    private ICustomNodeViewModel? _customNodeViewModel;
    private double _dragOffsetX;
    private double _dragOffsetY;
    private ObservableAsPropertyHelper<bool>? _hasInputPins;
    private ObservableAsPropertyHelper<bool>? _hasOutputPins;
    private bool _isSelected;

    private ObservableAsPropertyHelper<bool>? _isStaticNode;
    private double _startX;
    private double _startY;
    private bool _displayCustomViewModelAbove;
    private bool _displayCustomViewModelBetween;
    private bool _displayCustomViewModelBelow;
    private VerticalAlignment _customViewModelVerticalAlignment;

    public NodeViewModel(NodeScriptViewModel nodeScriptViewModel, INode node, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService, IWindowService windowService)
    {
        _nodeEditorService = nodeEditorService;
        _windowService = windowService;
        NodeScriptViewModel = nodeScriptViewModel;
        Node = node;

        SourceList<PinViewModel> nodePins = new();
        SourceList<PinCollectionViewModel> nodePinCollections = new();

        // Create observable collections split up by direction
        nodePins.Connect().Filter(n => n.Pin.Direction == PinDirection.Input).Bind(out ReadOnlyObservableCollection<PinViewModel> inputPins).Subscribe();
        nodePins.Connect().Filter(n => n.Pin.Direction == PinDirection.Output).Bind(out ReadOnlyObservableCollection<PinViewModel> outputPins).Subscribe();
        InputPinViewModels = inputPins;
        OutputPinViewModels = outputPins;

        // Same again but for pin collections
        nodePinCollections.Connect().Filter(n => n.PinCollection.Direction == PinDirection.Input).Bind(out ReadOnlyObservableCollection<PinCollectionViewModel> inputPinCollections).Subscribe();
        nodePinCollections.Connect().Filter(n => n.PinCollection.Direction == PinDirection.Output).Bind(out ReadOnlyObservableCollection<PinCollectionViewModel> outputPinCollections).Subscribe();
        InputPinCollectionViewModels = inputPinCollections;
        OutputPinCollectionViewModels = outputPinCollections;

        // Create a single observable collection containing all pin view models
        nodePins.Connect().Merge(nodePinCollections.Connect().TransformMany(c => c.PinViewModels)).Bind(out ReadOnlyObservableCollection<PinViewModel> pins).Subscribe();

        PinViewModels = pins;

        DeleteNode = ReactiveCommand.Create(ExecuteDeleteNode, this.WhenAnyValue(vm => vm.IsStaticNode).Select(v => !v));
        ShowBrokenState = ReactiveCommand.Create(ExecuteShowBrokenState);

        this.WhenActivated(d =>
        {
            _isStaticNode = Node.WhenAnyValue(n => n.IsDefaultNode, n => n.IsExitNode)
                .Select(tuple => tuple.Item1 || tuple.Item2)
                .ToProperty(this, model => model.IsStaticNode)
                .DisposeWith(d);
            _hasInputPins = InputPinViewModels.ToObservableChangeSet()
                .Merge(InputPinCollectionViewModels.ToObservableChangeSet().TransformMany(c => c.PinViewModels))
                .Any()
                .ToProperty(this, vm => vm.HasInputPins)
                .DisposeWith(d);
            _hasOutputPins = OutputPinViewModels.ToObservableChangeSet()
                .Merge(OutputPinCollectionViewModels.ToObservableChangeSet().TransformMany(c => c.PinViewModels))
                .Any()
                .ToProperty(this, vm => vm.HasOutputPins)
                .DisposeWith(d);

            // Subscribe to pin changes
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Node.PinAdded += x, x => Node.PinAdded -= x)
                .Subscribe(p =>
                {
                    if (p.EventArgs.Value.Direction == PinDirection.Input)
                        nodePins.Add(nodeVmFactory.InputPinViewModel(p.EventArgs.Value, nodeScriptViewModel));
                    else
                        nodePins.Add(nodeVmFactory.OutputPinViewModel(p.EventArgs.Value, nodeScriptViewModel));
                })
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Node.PinRemoved += x, x => Node.PinRemoved -= x)
                .Subscribe(p => nodePins.RemoveMany(nodePins.Items.Where(vm => vm.Pin == p.EventArgs.Value)))
                .DisposeWith(d);
            nodePins.Edit(l =>
            {
                l.Clear();
                foreach (IPin nodePin in Node.Pins)
                {
                    if (nodePin.Direction == PinDirection.Input)
                        l.Add(nodeVmFactory.InputPinViewModel(nodePin, nodeScriptViewModel));
                    else
                        l.Add(nodeVmFactory.OutputPinViewModel(nodePin, nodeScriptViewModel));
                }
            });

            // Subscribe to pin collection changes
            Observable.FromEventPattern<SingleValueEventArgs<IPinCollection>>(x => Node.PinCollectionAdded += x, x => Node.PinCollectionAdded -= x)
                .Subscribe(p =>
                {
                    if (p.EventArgs.Value.Direction == PinDirection.Input)
                        nodeVmFactory.InputPinCollectionViewModel(p.EventArgs.Value, nodeScriptViewModel);
                    else
                        nodeVmFactory.OutputPinCollectionViewModel(p.EventArgs.Value, nodeScriptViewModel);
                })
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPinCollection>>(x => Node.PinCollectionRemoved += x, x => Node.PinCollectionRemoved -= x)
                .Subscribe(p => nodePinCollections.RemoveMany(nodePinCollections.Items.Where(vm => vm.PinCollection == p.EventArgs.Value)))
                .DisposeWith(d);
            nodePinCollections.Edit(l =>
            {
                l.Clear();
                foreach (IPinCollection nodePinCollection in Node.PinCollections)
                {
                    if (nodePinCollection.Direction == PinDirection.Input)
                        l.Add(nodeVmFactory.InputPinCollectionViewModel(nodePinCollection, nodeScriptViewModel));
                    else
                        l.Add(nodeVmFactory.OutputPinCollectionViewModel(nodePinCollection, nodeScriptViewModel));
                }
            });

            // Set up the custom node VM if needed
            if (Node is ICustomViewModelNode customViewModelNode)
            {
                CustomNodeViewModel = customViewModelNode.GetCustomViewModel(nodeScriptViewModel.NodeScript);
                if (customViewModelNode.ViewModelPosition == CustomNodeViewModelPosition.AbovePins)
                    DisplayCustomViewModelAbove = true;
                else if (customViewModelNode.ViewModelPosition == CustomNodeViewModelPosition.BelowPins)
                    DisplayCustomViewModelBelow = true;
                else
                {
                    DisplayCustomViewModelBetween = true;
                    
                    if (customViewModelNode.ViewModelPosition == CustomNodeViewModelPosition.BetweenPinsTop)
                        CustomViewModelVerticalAlignment = VerticalAlignment.Top;
                    else if (customViewModelNode.ViewModelPosition == CustomNodeViewModelPosition.BetweenPinsTop)
                        CustomViewModelVerticalAlignment = VerticalAlignment.Center;
                    else
                        CustomViewModelVerticalAlignment = VerticalAlignment.Bottom;
                }
            }
        });
    }

    public bool IsStaticNode => _isStaticNode?.Value ?? true;
    public bool HasInputPins => _hasInputPins?.Value ?? false;
    public bool HasOutputPins => _hasOutputPins?.Value ?? false;
    public NodeScriptViewModel NodeScriptViewModel { get; }
    public INode Node { get; }
    public ReadOnlyObservableCollection<PinViewModel> InputPinViewModels { get; }
    public ReadOnlyObservableCollection<PinCollectionViewModel> InputPinCollectionViewModels { get; }
    public ReadOnlyObservableCollection<PinViewModel> OutputPinViewModels { get; }
    public ReadOnlyObservableCollection<PinCollectionViewModel> OutputPinCollectionViewModels { get; }
    public ReadOnlyObservableCollection<PinViewModel> PinViewModels { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }
    
    public ICustomNodeViewModel? CustomNodeViewModel
    {
        get => _customNodeViewModel;
        set => RaiseAndSetIfChanged(ref _customNodeViewModel, value);
    }

    public bool DisplayCustomViewModelAbove
    {
        get => _displayCustomViewModelAbove;
        set => RaiseAndSetIfChanged(ref _displayCustomViewModelAbove, value);
    }

    public bool DisplayCustomViewModelBetween
    {
        get => _displayCustomViewModelBetween;
        set => RaiseAndSetIfChanged(ref _displayCustomViewModelBetween, value);
    }

    public bool DisplayCustomViewModelBelow
    {
        get => _displayCustomViewModelBelow;
        set => RaiseAndSetIfChanged(ref _displayCustomViewModelBelow, value);
    }

    public VerticalAlignment CustomViewModelVerticalAlignment
    {
        get => _customViewModelVerticalAlignment;
        set => RaiseAndSetIfChanged(ref _customViewModelVerticalAlignment, value);
    }

    public ReactiveCommand<Unit, Unit> ShowBrokenState { get; }
    public ReactiveCommand<Unit, Unit> DeleteNode { get; }

    public void StartDrag(Point mouseStartPosition)
    {
        if (!IsSelected)
            return;

        _dragOffsetX = Node.X - mouseStartPosition.X;
        _dragOffsetY = Node.Y - mouseStartPosition.Y;
        _startX = Node.X;
        _startY = Node.Y;
    }

    public void UpdateDrag(Point mousePosition)
    {
        if (!IsSelected)
            return;

        Node.X = Math.Round((mousePosition.X + _dragOffsetX) / 10d, 0, MidpointRounding.AwayFromZero) * 10d;
        Node.Y = Math.Round((mousePosition.Y + _dragOffsetY) / 10d, 0, MidpointRounding.AwayFromZero) * 10d;
    }

    public MoveNode? FinishDrag()
    {
        if (IsSelected && (Math.Abs(_startX - Node.X) > 0.01 || Math.Abs(_startY - Node.Y) > 0.01))
            return new MoveNode(Node, Node.X, Node.Y, _startX, _startY);
        return null;
    }

    private void ExecuteDeleteNode()
    {
        _nodeEditorService.ExecuteCommand(NodeScriptViewModel.NodeScript, new DeleteNode(NodeScriptViewModel.NodeScript, Node));
    }

    private void ExecuteShowBrokenState()
    {
        if (Node.BrokenState != null && Node.BrokenStateException != null)
            _windowService.ShowExceptionDialog(Node.BrokenState, Node.BrokenStateException);
    }
}