using System;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using Artemis.UI.Shared;
using DynamicData;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public class PropertyViewModel : PropertyViewModelBase, IDisposable
{
    private bool _isExpanded;
    private bool _isHighlighted;
    private bool _isVisible;
    private readonly SourceList<ILayerPropertyKeyframe> _keyframes;

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