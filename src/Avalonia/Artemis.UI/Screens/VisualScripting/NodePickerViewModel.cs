using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodePickerViewModel : ActivatableViewModelBase
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly NodeScript _nodeScript;
    private readonly INodeService _nodeService;

    private bool _isVisible;
    private Point _position;
    private string? _searchText;
    private NodeData? _selectedNode;

    public NodePickerViewModel(NodeScript nodeScript, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeScript = nodeScript;
        _nodeService = nodeService;
        _nodeEditorService = nodeEditorService;

        Nodes = new ObservableCollection<NodeData>(_nodeService.AvailableNodes);

        this.WhenActivated(d =>
        {
            IsVisible = true;
            Disposable.Create(() => IsVisible = false).DisposeWith(d);
        });

        this.WhenAnyValue(vm => vm.SelectedNode).WhereNotNull().Throttle(TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler).Subscribe(data =>
        {
            CreateNode(data);
            Hide();
            SelectedNode = null;
        });
    }

    public ObservableCollection<NodeData> Nodes { get; }

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

    public NodeData? SelectedNode
    {
        get => _selectedNode;
        set => RaiseAndSetIfChanged(ref _selectedNode, value);
    }

    public void Show(Point position)
    {
        IsVisible = true;
        Position = position;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private void CreateNode(NodeData data)
    {
        INode node = data.CreateNode(_nodeScript, null);
        node.X = Position.X;
        node.Y = Position.Y;

        _nodeEditorService.ExecuteCommand(_nodeScript, new AddNode(_nodeScript, node));
    }
}