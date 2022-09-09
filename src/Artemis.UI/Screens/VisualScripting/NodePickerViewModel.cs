using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using DynamicData;
using DynamicData.Binding;
using FuzzySharp;
using FuzzySharp.Extractor;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodePickerViewModel : ActivatableViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _isSearching;
    private readonly INodeEditorService _nodeEditorService;
    private readonly NodeScript _nodeScript;

    private bool _isVisible;
    private Point _position;
    private string? _searchText;
    private object? _selectedNode;
    private IPin? _targetPin;

    public NodePickerViewModel(NodeScript nodeScript, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeScript = nodeScript;
        _nodeEditorService = nodeEditorService;

        SourceList<NodeDataViewModel> nodeSourceList = new();
        IObservable<Func<NodeDataViewModel, bool>> pinFilter = this.WhenAnyValue(vm => vm.TargetPin).Select(CreateTargetPinPredicate);
        nodeSourceList.Connect()
            // If spawning for a pin, only show compatible nodes
            .Filter(pinFilter)
            // Put data model and static on top, then sort by category, name and description
            .Sort(SortExpressionComparer<NodeDataViewModel>
                .Descending(d => d.NodeData.Category == "Data Model")
                .ThenByDescending(d => d.NodeData.Category == "Static")
                .ThenByAscending(d => d.NodeData.Category)
                .ThenByAscending(d => d.NodeData.Name)
                .ThenByAscending(d => d.NodeData.Description))
            // Group by category
            .GroupWithImmutableState(d => d.NodeData.Category)
            .Bind(out ReadOnlyObservableCollection<DynamicData.List.IGrouping<NodeDataViewModel, string>> categories)
            .Subscribe();

        IObservable<Unit> resortObservable = nodeSourceList.Connect()
            .WhenPropertyChanged(vm => vm.SearchScore)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Select(_ => Unit.Default);

        nodeSourceList.Connect()
            .AutoRefresh(model => model.SearchScore)
            // If spawning for a pin, only show compatible nodes
            .Filter(pinFilter)
            // Limit the search results to the average of the top 33%
            .Filter(model =>
            {
                // Wasteful, I know
                List<NodeDataViewModel> nodes = nodeSourceList.Items.OrderByDescending(s => s.SearchScore).ToList();
                return model.SearchScore >= nodes.Take(nodes.Count / 3).Average(i => i.SearchScore);
            })
            // Sort on the highest score
            .Sort(SortExpressionComparer<NodeDataViewModel>.Descending(data => data.SearchScore).ThenByDescending(d => d.NodeData.Name.Length), resort: resortObservable)
            .Bind(out ReadOnlyObservableCollection<NodeDataViewModel> sortedNodes)
            .Subscribe();
        Categories = categories;
        SortedNodes = sortedNodes;

        _isSearching = this.WhenAnyValue(vm => vm.SearchText).Select(s => !string.IsNullOrWhiteSpace(s)).ToProperty(this, vm => vm.IsSearching);

        this.WhenAnyValue(vm => vm.SearchText).Subscribe(s =>
        {
            if (s == null)
                return;

            List<ExtractedResult<string>> scoredByName = Process.ExtractSorted(s, nodeService.AvailableNodes.Select(d => d.Name)).ToList();
            List<ExtractedResult<string>> scoredByDescription = Process.ExtractSorted(s, nodeService.AvailableNodes.Select(d => d.Description)).ToList();
            foreach (NodeDataViewModel viewModel in nodeSourceList.Items)
                viewModel.SetSearchScore(scoredByName, scoredByDescription);
        });

        this.WhenActivated(d =>
        {
            SearchText = null;
            TargetPin = null;

            nodeSourceList.Edit(list =>
            {
                list.Clear();
                list.AddRange(nodeService.AvailableNodes.Select(n => new NodeDataViewModel(n)));
            });

            IsVisible = true;
            Disposable.Create(() => IsVisible = false).DisposeWith(d);
        });
    }

    public ReadOnlyObservableCollection<DynamicData.List.IGrouping<NodeDataViewModel, string>> Categories { get; }
    public ReadOnlyObservableCollection<NodeDataViewModel> SortedNodes { get; }
    public bool IsSearching => _isSearching.Value;

    public bool IsVisible
    {
        get => _isVisible;
        set => RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public Point Position
    {
        get => _position;
        set => RaiseAndSetIfChanged(ref _position, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public IPin? TargetPin
    {
        get => _targetPin;
        set => RaiseAndSetIfChanged(ref _targetPin, value);
    }

    public object? SelectedNode
    {
        get => _selectedNode;
        set => RaiseAndSetIfChanged(ref _selectedNode, value);
    }

    public void CreateNode(NodeDataViewModel viewModel)
    {
        INode node = viewModel.NodeData.CreateNode(_nodeScript, null);
        node.X = Math.Round(Position.X / 10d, 0, MidpointRounding.AwayFromZero) * 10d;
        node.Y = Math.Round(Position.Y / 10d, 0, MidpointRounding.AwayFromZero) * 10d;

        if (TargetPin != null)
            using (_nodeEditorService.CreateCommandScope(_nodeScript, "Create node for pin"))
            {
                _nodeEditorService.ExecuteCommand(_nodeScript, new AddNode(_nodeScript, node));

                // Find the first compatible source pin for the target pin
                IPin? source = TargetPin.Direction == PinDirection.Output
                    ? node.Pins.Concat(node.PinCollections.SelectMany(c => c)).Where(p => p.Direction == PinDirection.Input).FirstOrDefault(p => TargetPin.IsTypeCompatible(p.Type))
                    : node.Pins.Concat(node.PinCollections.SelectMany(c => c)).Where(p => p.Direction == PinDirection.Output).FirstOrDefault(p => TargetPin.IsTypeCompatible(p.Type));

                if (source != null)
                    _nodeEditorService.ExecuteCommand(_nodeScript, new ConnectPins(source, TargetPin));
            }
        else
            _nodeEditorService.ExecuteCommand(_nodeScript, new AddNode(_nodeScript, node));
    }

    private Func<NodeDataViewModel, bool> CreateTargetPinPredicate(IPin? targetPin)
    {
        return vm => vm.NodeData.IsCompatibleWithPin(targetPin);
    }
}