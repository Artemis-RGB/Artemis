using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly INodeService _nodeService;

    private bool _isVisible;
    private Point _position;
    private List<ExtractedResult<string>>? _scoredByDescription;
    private List<ExtractedResult<string>>? _scoredByName;
    private string? _searchText;
    private object? _selectedNode;
    private IPin? _targetPin;

    public NodePickerViewModel(NodeScript nodeScript, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeScript = nodeScript;
        _nodeService = nodeService;
        _nodeEditorService = nodeEditorService;

        SourceList<NodeData> nodeSourceList = new();
        IObservable<Func<NodeData, bool>> pinFilter = this.WhenAnyValue(vm => vm.TargetPin).Select(CreateTargetPinPredicate);
        IObservable<Func<NodeData, bool>> scoreFilter = this.WhenAnyValue(vm => vm.SearchText).Select(CreateScorePredicate);

        nodeSourceList.Connect()
            .Filter(pinFilter)
            .Sort(SortExpressionComparer<NodeData>
                .Descending(d => d.Category == "Data Model")
                .ThenByDescending(d => d.Category == "Static")
                .ThenByAscending(d => d.Category)
                .ThenByAscending(d => d.Name)
                .ThenByAscending(d => d.Description))
            .GroupWithImmutableState(n => n.Category)
            .Bind(out ReadOnlyObservableCollection<DynamicData.List.IGrouping<NodeData, string>> categories)
            .Subscribe();
        nodeSourceList.Connect()
            .Filter(pinFilter)
            .Filter(scoreFilter)
            .AutoRefreshOnObservable(_ => this.WhenAnyValue(vm => vm.SearchText))
            // Sort on the highest score
            .Sort(SortExpressionComparer<NodeData>
                .Descending(data => Math.Max(
                        _scoredByName?.FirstOrDefault(n => n.Value == data.Name)?.Score ?? -1,
                        _scoredByDescription?.FirstOrDefault(n => n.Value == data.Description)?.Score ?? -1) / 2
                ))
            .Bind(out ReadOnlyObservableCollection<NodeData> sortedNodes)
            .Subscribe();
        Categories = categories;
        SortedNodes = sortedNodes;

        _isSearching = this.WhenAnyValue(vm => vm.SearchText).Select(s => !string.IsNullOrWhiteSpace(s)).ToProperty(this, vm => vm.IsSearching);

        this.WhenActivated(d =>
        {
            SearchText = null;
            TargetPin = null;

            nodeSourceList.Edit(list =>
            {
                list.Clear();
                list.AddRange(nodeService.AvailableNodes);
            });

            IsVisible = true;
            Disposable.Create(() => IsVisible = false).DisposeWith(d);
        });
    }

    public ReadOnlyObservableCollection<DynamicData.List.IGrouping<NodeData, string>> Categories { get; }
    public ReadOnlyObservableCollection<NodeData> SortedNodes { get; }
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

    public void CreateNode(NodeData data)
    {
        INode node = data.CreateNode(_nodeScript, null);
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

    private Func<NodeData, bool> CreateTargetPinPredicate(IPin? targetPin)
    {
        return data => data.IsCompatibleWithPin(targetPin);
    }

    private Func<NodeData, bool> CreateScorePredicate(string? search)
    {
        if (search == null)
            return _ => false;

        _scoredByName = Process.ExtractSorted(search, _nodeService.AvailableNodes.Select(d => d.Name)).ToList();
        _scoredByDescription = Process.ExtractSorted(search, _nodeService.AvailableNodes.Select(d => d.Description)).ToList();

        // Take the top 25% of each
        double nameCutOff = _scoredByName.Take((int) (_scoredByName.Count / 100.0 * 25)).Average(s => s.Score);
        double descriptionCutOff = _scoredByDescription.Take((int) (_scoredByName.Count / 100.0 * 25)).Average(s => s.Score) / 2;
        return data =>
        {
            int nameScore = _scoredByName.FirstOrDefault(s => s.Value == data.Name)?.Score ?? -1;
            int descriptionScore = (_scoredByDescription.FirstOrDefault(s => s.Value == data.Description)?.Score ?? -1) / 2;

            // Use the best score
            if (nameScore >= descriptionScore)
                return nameScore >= nameCutOff;
            return descriptionScore >= descriptionCutOff;
        };
    }
}