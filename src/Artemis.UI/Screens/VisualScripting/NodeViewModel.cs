﻿using System;
using System.Collections.ObjectModel;
using System.Reactive;
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

public class NodeViewModel : ActivatableViewModelBase
{
    private readonly INodeEditorService _nodeEditorService;

    private ICustomNodeViewModel? _customNodeViewModel;
    private ReactiveCommand<Unit, Unit>? _deleteNode;
    private double _dragOffsetX;
    private double _dragOffsetY;
    private ObservableAsPropertyHelper<bool>? _hasInputPins;
    private ObservableAsPropertyHelper<bool>? _hasOutputPins;
    private bool _isSelected;

    private ObservableAsPropertyHelper<bool>? _isStaticNode;
    private double _startX;
    private double _startY;

    public NodeViewModel(NodeScriptViewModel nodeScriptViewModel, INode node, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
    {
        _nodeEditorService = nodeEditorService;
        NodeScriptViewModel = nodeScriptViewModel;
        Node = node;

        DeleteNode = ReactiveCommand.Create(ExecuteDeleteNode, this.WhenAnyValue(vm => vm.IsStaticNode).Select(v => !v));

        SourceList<IPin> nodePins = new();
        SourceList<IPinCollection> nodePinCollections = new();

        // Create observable collections split up by direction
        nodePins.Connect()
            .Filter(n => n.Direction == PinDirection.Input)
            .Transform(p => (PinViewModel) nodeVmFactory.InputPinViewModel(p, nodeScriptViewModel))
            .Bind(out ReadOnlyObservableCollection<PinViewModel> inputPins)
            .Subscribe();
        nodePins.Connect()
            .Filter(n => n.Direction == PinDirection.Output)
            .Transform(p => (PinViewModel) nodeVmFactory.OutputPinViewModel(p, nodeScriptViewModel))
            .Bind(out ReadOnlyObservableCollection<PinViewModel> outputPins)
            .Subscribe();
        InputPinViewModels = inputPins;
        OutputPinViewModels = outputPins;

        // Same again but for pin collections
        nodePinCollections.Connect()
            .Filter(n => n.Direction == PinDirection.Input)
            .Transform(c => (PinCollectionViewModel) nodeVmFactory.InputPinCollectionViewModel(c, nodeScriptViewModel))
            .Bind(out ReadOnlyObservableCollection<PinCollectionViewModel> inputPinCollections)
            .Subscribe();
        nodePinCollections.Connect()
            .Filter(n => n.Direction == PinDirection.Output)
            .Transform(c => (PinCollectionViewModel) nodeVmFactory.OutputPinCollectionViewModel(c, nodeScriptViewModel))
            .Bind(out ReadOnlyObservableCollection<PinCollectionViewModel> outputPinCollections)
            .Subscribe();
        InputPinCollectionViewModels = inputPinCollections;
        OutputPinCollectionViewModels = outputPinCollections;

        // Create a single observable collection containing all pin view models
        InputPinViewModels.ToObservableChangeSet()
            .Merge(OutputPinViewModels.ToObservableChangeSet())
            .Merge(InputPinCollectionViewModels.ToObservableChangeSet().TransformMany(c => c.PinViewModels))
            .Merge(OutputPinCollectionViewModels.ToObservableChangeSet().TransformMany(c => c.PinViewModels))
            .Bind(out ReadOnlyObservableCollection<PinViewModel> pins)
            .Subscribe();

        PinViewModels = pins;

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
                .Subscribe(p => nodePins.Add(p.EventArgs.Value))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Node.PinRemoved += x, x => Node.PinRemoved -= x)
                .Subscribe(p => nodePins.Remove(p.EventArgs.Value))
                .DisposeWith(d);
            nodePins.Edit(l =>
            {
                l.Clear();
                l.AddRange(Node.Pins);
            });
            
            // Subscribe to pin collection changes
            Observable.FromEventPattern<SingleValueEventArgs<IPinCollection>>(x => Node.PinCollectionAdded += x, x => Node.PinCollectionAdded -= x)
                .Subscribe(p => nodePinCollections.Add(p.EventArgs.Value))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPinCollection>>(x => Node.PinCollectionRemoved += x, x => Node.PinCollectionRemoved -= x)
                .Subscribe(p => nodePinCollections.Remove(p.EventArgs.Value))
                .DisposeWith(d);
            nodePinCollections.Edit(l =>
            {
                l.Clear();
                l.AddRange(Node.PinCollections);
            });
            
            if (Node is Node coreNode)
                CustomNodeViewModel = coreNode.GetCustomViewModel(nodeScriptViewModel.NodeScript);
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

    public ICustomNodeViewModel? CustomNodeViewModel
    {
        get => _customNodeViewModel;
        set => RaiseAndSetIfChanged(ref _customNodeViewModel, value);
    }

    public ReactiveCommand<Unit, Unit>? DeleteNode
    {
        get => _deleteNode;
        set => RaiseAndSetIfChanged(ref _deleteNode, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

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
}