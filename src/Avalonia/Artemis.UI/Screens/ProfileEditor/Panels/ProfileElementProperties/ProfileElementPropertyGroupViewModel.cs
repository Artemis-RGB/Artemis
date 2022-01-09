using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;

public class ProfileElementPropertyGroupViewModel : ViewModelBase
{
    private bool _isVisible;
    private bool _isExpanded;

    public ProfileElementPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory)
    {
        Children = new ObservableCollection<ActivatableViewModelBase>();
        LayerPropertyGroup = layerPropertyGroup;
        TreeGroupViewModel = layerPropertyVmFactory.TreeGroupViewModel(this);

        IsVisible = !LayerPropertyGroup.IsHidden;
        // TODO: Update visiblity on change, can't do it atm because not sure how to unsubscribe from the event
    }

    public ObservableCollection<ActivatableViewModelBase> Children { get; }
    public LayerPropertyGroup LayerPropertyGroup { get; }
    public TreeGroupViewModel TreeGroupViewModel { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }
}