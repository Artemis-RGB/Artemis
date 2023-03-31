using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;
using static Artemis.VisualScripting.Nodes.Input.PressedKeyPositionNodeEntity;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public class PressedKeyPositionNodeCustomViewModel : CustomNodeViewModel
{
    private readonly INodeEditorService _nodeEditorService;
    private readonly Profile? _profile;
    private readonly PressedKeyPositionNode _node;
    private Layer? _selectedLayer;
    private KeyPressType _respondTo;

    public PressedKeyPositionNodeCustomViewModel(PressedKeyPositionNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _nodeEditorService = nodeEditorService;
        _profile = script.Context as Profile;
        _node = node;

        Layers = new ObservableCollection<Layer>();

        this.WhenActivated(d =>
        {
            if (_profile == null)
                return;

            Observable.FromEventPattern<ProfileElementEventArgs>(x => _profile.DescendentAdded += x, x => _profile.DescendentAdded -= x).Subscribe(_ => GetLayers()).DisposeWith(d);
            Observable.FromEventPattern<ProfileElementEventArgs>(x => _profile.DescendentRemoved += x, x => _profile.DescendentRemoved -= x).Subscribe(_ => GetLayers()).DisposeWith(d);
            Observable.FromEventPattern(x => _node.StorageModified += x, x => _node.StorageModified -= x).Subscribe(_ => Update()).DisposeWith(d);

            GetLayers();
        });

        this.WhenAnyValue(vm => vm.SelectedLayer).Subscribe(UpdateSelectedLayer);
        this.WhenAnyValue(vm => vm.RespondTo).Subscribe(UpdateSelectedRespondTo);
    }

    public ObservableCollection<Layer> Layers { get; }

    public Layer? SelectedLayer
    {
        get => _selectedLayer;
        set => this.RaiseAndSetIfChanged(ref _selectedLayer, value);
    }

    public KeyPressType RespondTo
    {
        get => _respondTo;
        set => this.RaiseAndSetIfChanged(ref _respondTo, value);
    }

    private void GetLayers()
    {
        Layers.Clear();
        if (_profile == null)
            return;
        foreach (Layer layer in _profile.GetAllLayers())
            Layers.Add(layer);

        Update();
    }

    private void Update()
    {
        SelectedLayer = Layers.FirstOrDefault(l => l.EntityId == _node.Storage?.LayerId);
        RespondTo = _node.Storage?.RespondTo ?? KeyPressType.Up;
    }
    
    private void UpdateSelectedLayer(Layer? layer)
    {
        if (layer == null || _node.Storage?.LayerId == layer.EntityId)
            return;

        _nodeEditorService.ExecuteCommand(
            Script,
            new UpdateStorage<PressedKeyPositionNodeEntity>(_node, new PressedKeyPositionNodeEntity(layer.EntityId, _node.Storage?.RespondTo ?? KeyPressType.Up), "layer")
        );
    }

    private void UpdateSelectedRespondTo(KeyPressType respondTo)
    {
        if (_node.Storage?.RespondTo == respondTo)
            return;

        _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<PressedKeyPositionNodeEntity>(_node, new PressedKeyPositionNodeEntity(_node.Storage?.LayerId ?? Guid.Empty, respondTo), "layer"));
    }
}