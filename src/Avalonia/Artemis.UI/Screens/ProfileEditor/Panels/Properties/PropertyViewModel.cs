using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public class PropertyViewModel : ViewModelBase
{
    private bool _isExpanded;
    private bool _isHighlighted;
    private bool _isVisible;

    public PropertyViewModel(ILayerProperty layerProperty, IPropertyVmFactory propertyVmFactory)
    {
        LayerProperty = layerProperty;
        TreePropertyViewModel = propertyVmFactory.TreePropertyViewModel(LayerProperty, this);
        TimelinePropertyViewModel = propertyVmFactory.TimelinePropertyViewModel(LayerProperty, this);

        // TODO: Centralize visibility updating or do it here and dispose
        _isVisible = !LayerProperty.IsHidden;
    }

    public ILayerProperty LayerProperty { get; }
    public ITreePropertyViewModel TreePropertyViewModel { get; }
    public ITimelinePropertyViewModel TimelinePropertyViewModel { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set => RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => RaiseAndSetIfChanged(ref _isHighlighted, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => RaiseAndSetIfChanged(ref _isExpanded, value);
    }
}