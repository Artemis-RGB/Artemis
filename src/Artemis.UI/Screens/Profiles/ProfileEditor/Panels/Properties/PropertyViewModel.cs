using System;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Tree;
using DynamicData;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties;

public partial class PropertyViewModel : PropertyViewModelBase, IDisposable
{
    private readonly SourceList<ILayerPropertyKeyframe> _keyframes;
    [Notify] private bool _isExpanded;
    [Notify] private bool _isHighlighted;
    [Notify] private bool _isVisible;

    public PropertyViewModel(ILayerProperty layerProperty, IPropertyVmFactory propertyVmFactory)
    {
        LayerProperty = layerProperty;
        TreePropertyViewModel = propertyVmFactory.TreePropertyViewModel(LayerProperty, this);
        TimelinePropertyViewModel = propertyVmFactory.TimelinePropertyViewModel(LayerProperty, this);

        _isVisible = !LayerProperty.IsHidden;
        _keyframes = new SourceList<ILayerPropertyKeyframe>();
        _keyframes.Edit(k => k.AddRange(LayerProperty.UntypedKeyframes));
        _keyframes.Connect().Bind(out ReadOnlyObservableCollection<ILayerPropertyKeyframe> keyframes).Subscribe();

        Keyframes = keyframes;

        LayerProperty.VisibilityChanged += LayerPropertyOnVisibilityChanged;
        LayerProperty.KeyframeAdded += LayerPropertyOnKeyframeAdded;
        LayerProperty.KeyframeRemoved += LayerPropertyOnKeyframeRemoved;
    }

    public ILayerProperty LayerProperty { get; }
    public ITreePropertyViewModel TreePropertyViewModel { get; }
    public ITimelinePropertyViewModel TimelinePropertyViewModel { get; }
    public override ReadOnlyObservableCollection<ILayerPropertyKeyframe> Keyframes { get; }

    private void LayerPropertyOnVisibilityChanged(object? sender, LayerPropertyEventArgs e)
    {
        IsVisible = !LayerProperty.IsHidden;
    }

    private void LayerPropertyOnKeyframeAdded(object? sender, LayerPropertyKeyframeEventArgs e)
    {
        _keyframes.Add(e.Keyframe);
    }

    private void LayerPropertyOnKeyframeRemoved(object? sender, LayerPropertyKeyframeEventArgs e)
    {
        _keyframes.Remove(e.Keyframe);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        LayerProperty.VisibilityChanged -= LayerPropertyOnVisibilityChanged;
        LayerProperty.KeyframeAdded -= LayerPropertyOnKeyframeAdded;
        LayerProperty.KeyframeRemoved -= LayerPropertyOnKeyframeRemoved;
    }
}