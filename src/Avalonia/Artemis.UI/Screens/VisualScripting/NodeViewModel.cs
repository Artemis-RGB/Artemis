using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeViewModel : ActivatableViewModelBase
{
    private readonly NodeScript _nodeScript;
    private readonly INodeEditorService _nodeEditorService;

    private ICustomNodeViewModel? _customNodeViewModel;
    private ReactiveCommand<Unit, Unit>? _deleteNode;
    private ObservableAsPropertyHelper<bool>? _isStaticNode;

    public NodeViewModel(NodeScript nodeScript, INode node, INodePinVmFactory nodePinVmFactory, INodeEditorService nodeEditorService)
    {
        _nodeScript = nodeScript;
        _nodeEditorService = nodeEditorService;
        Node = node;

        SourceList<IPin> nodePins = new();
        this.WhenActivated(d =>
        {
            _isStaticNode = Node.WhenAnyValue(n => n.IsDefaultNode, n => n.IsExitNode)
                .Select(tuple => tuple.Item1 || tuple.Item2)
                .ToProperty(this, model => model.IsStaticNode)
                .DisposeWith(d);

            Node.WhenAnyValue(n => n.Pins).Subscribe(pins => nodePins.Edit(source =>
            {
                source.Clear();
                source.AddRange(pins);
            })).DisposeWith(d);
        });

        DeleteNode = ReactiveCommand.Create(ExecuteDeleteNode, this.WhenAnyValue(vm => vm.IsStaticNode).Select(v => !v));

        nodePins.Connect().Filter(n => n.Direction == PinDirection.Input).Transform(nodePinVmFactory.InputPinViewModel).Bind(out ReadOnlyObservableCollection<PinViewModel> inputPins).Subscribe();
        nodePins.Connect().Filter(n => n.Direction == PinDirection.Output).Transform(nodePinVmFactory.OutputPinViewModel).Bind(out ReadOnlyObservableCollection<PinViewModel> outputPins).Subscribe();
        InputPinViewModels = inputPins;
        OutputPinViewModels = outputPins;
    }

    public bool IsStaticNode => _isStaticNode?.Value ?? true;

    public INode Node { get; }
    public ReadOnlyObservableCollection<PinViewModel> InputPinViewModels { get; }
    public ReadOnlyObservableCollection<PinCollectionViewModel> InputPinCollectionViewModels { get; }
    public ReadOnlyObservableCollection<PinViewModel> OutputPinViewModels { get; }
    public ReadOnlyObservableCollection<PinCollectionViewModel> OutputPinCollectionViewModels { get; }

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

    private void ExecuteDeleteNode()
    {
        _nodeEditorService.ExecuteCommand(_nodeScript, new DeleteNode(_nodeScript, Node));
    }
}