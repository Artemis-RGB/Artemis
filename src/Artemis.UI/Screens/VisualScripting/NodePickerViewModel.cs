using System;
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
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodePickerViewModel : ActivatableViewModelBase
{
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

        SourceList<NodeData> nodeSourceList = new();
        IObservable<Func<NodeData, bool>> nodeFilter = this.WhenAnyValue(vm => vm.SearchText, vm => vm.TargetPin).Select(v => CreatePredicate(v.Item1, v.Item2));

        nodeSourceList.Connect()
            .Filter(nodeFilter)
            .Sort(SortExpressionComparer<NodeData>
                .Descending(d => d.Category == "Data Model")
                .ThenByDescending(d => d.Category == "Static")
                .ThenByAscending(d => d.Category)
                .ThenByAscending(d => d.Name))
            .GroupWithImmutableState(n => n.Category)
            .Bind(out ReadOnlyObservableCollection<DynamicData.List.IGrouping<NodeData, string>> categories)
            .Subscribe();
        Categories = categories;

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

    private Func<NodeData, bool> CreatePredicate(string? text, IPin? targetPin)
    {
        if (string.IsNullOrWhiteSpace(text))
            return data => data.IsCompatibleWithPin(targetPin);
        return data => data.IsCompatibleWithPin(targetPin) && data.MatchesSearch(text);
    }
}