using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.VisualScripting;
using Artemis.VisualScripting.Nodes.External.Commands;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.External.Screens;

public class LayerPropertyNodeCustomViewModel : CustomNodeViewModel
{
    private readonly LayerPropertyNode _node;
    private readonly INodeEditorService _nodeEditorService;
    private bool _outsideProfileContext;

    public LayerPropertyNodeCustomViewModel(LayerPropertyNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;
        this.WhenActivated(d =>
        {
            if (_node.Script?.Context is not Profile profile)
            {
                OutsideProfileContext = true;
                return;
            }

            OutsideProfileContext = false;
            Observable.FromEventPattern<ProfileElementEventArgs>(x => profile.DescendentAdded += x, x => profile.DescendentAdded -= x).Subscribe(_ => GetProfileElements()).DisposeWith(d);
            Observable.FromEventPattern<ProfileElementEventArgs>(x => profile.DescendentRemoved += x, x => profile.DescendentRemoved -= x).Subscribe(_ => GetProfileElements()).DisposeWith(d);
            GetProfileElements();
            GetLayerProperties();
        });

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(SelectedProfileElement));
        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(SelectedLayerProperty));
        this.WhenAnyValue(vm => vm.SelectedProfileElement).Subscribe(_ => GetLayerProperties());
    }

    public ObservableCollection<RenderProfileElement> ProfileElements { get; } = new();
    public ObservableCollection<ILayerProperty> LayerProperties { get; } = new();

    public bool OutsideProfileContext
    {
        get => _outsideProfileContext;
        set => this.RaiseAndSetIfChanged(ref _outsideProfileContext, value);
    }

    public RenderProfileElement? SelectedProfileElement
    {
        get => _node.ProfileElement;
        set
        {
            if (value != null && !Equals(_node.ProfileElement, value))
                _nodeEditorService.ExecuteCommand(Script, new UpdateLayerPropertyNodeSelectedProfileElement(_node, value));
        }
    }

    public ILayerProperty? SelectedLayerProperty
    {
        get => _node.LayerProperty;
        set
        {
            if (value != null && !Equals(_node.LayerProperty, value))
                _nodeEditorService.ExecuteCommand(Script, new UpdateLayerPropertyNodeSelectedLayerProperty(_node, value));
        }
    }

    private void GetProfileElements()
    {
        ProfileElements.Clear();
        if (_node.Script?.Context is not Profile profile)
            return;

        List<RenderProfileElement> elements = new(profile.GetAllRenderElements());

        ProfileElements.AddRange(elements.OrderBy(e => e.Order));
        SelectedProfileElement = _node.ProfileElement;
    }

    private void GetLayerProperties()
    {
        LayerProperties.Clear();
        if (_node.ProfileElement == null)
            return;

        LayerProperties.AddRange(_node.ProfileElement.GetAllLayerProperties().Where(l => !l.IsHidden && l.DataBindingsSupported));
        SelectedLayerProperty = _node.LayerProperty;
    }
}