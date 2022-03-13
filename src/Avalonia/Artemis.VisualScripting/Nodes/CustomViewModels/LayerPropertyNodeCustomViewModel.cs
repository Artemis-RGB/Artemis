using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.CustomViewModels;

public class LayerPropertyNodeCustomViewModel : CustomNodeViewModel
{
    private readonly LayerPropertyNode _node;
    private ILayerProperty _selectedLayerProperty;

    private RenderProfileElement _selectedProfileElement;

    public LayerPropertyNodeCustomViewModel(LayerPropertyNode node) : base(node)
    {
        _node = node;
    }

    public ObservableCollection<RenderProfileElement> ProfileElements { get; } = new();

    // public RenderProfileElement SelectedProfileElement
    // {
    //     get => _selectedProfileElement;
    //     set
    //     {
    //         if (!SetAndNotify(ref _selectedProfileElement, value)) return;
    //         _node.ChangeProfileElement(_selectedProfileElement);
    //         GetLayerProperties();
    //     }
    // }

    public ObservableCollection<ILayerProperty> LayerProperties { get; } = new();

    // public ILayerProperty SelectedLayerProperty
    // {
    //     get => _selectedLayerProperty;
    //     set
    //     {
    //         if (!SetAndNotify(ref _selectedLayerProperty, value)) return;
    //         _node.ChangeLayerProperty(_selectedLayerProperty);
    //     }
    // }

    // private void GetProfileElements()
    // {
    //     ProfileElements.Clear();
    //     if (_node.Script.Context is not Profile profile)
    //         return;
    //
    //     List<RenderProfileElement> elements = new(profile.GetAllRenderElements());
    //
    //     ProfileElements.AddRange(elements.OrderBy(e => e.Order));
    //     _selectedProfileElement = _node.ProfileElement;
    //     NotifyOfPropertyChange(nameof(SelectedProfileElement));
    // }
    //
    // private void GetLayerProperties()
    // {
    //     LayerProperties.Clear();
    //     if (_node.ProfileElement == null)
    //         return;
    //
    //     LayerProperties.AddRange(_node.ProfileElement.GetAllLayerProperties().Where(l => !l.IsHidden && l.DataBindingsSupported));
    //     _selectedLayerProperty = _node.LayerProperty;
    //     NotifyOfPropertyChange(nameof(SelectedLayerProperty));
    // }
    //
    // public override void OnActivate()
    // {
    //     GetProfileElements();
    //     GetLayerProperties();
    // }
}