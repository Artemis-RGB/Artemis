using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Keyframes;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Tree;
using Artemis.UI.Shared.Services.PropertyInput;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties;

public partial class PropertyGroupViewModel : PropertyViewModelBase, IDisposable
{
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private readonly IPropertyInputService _propertyInputService;
    private ReadOnlyObservableCollection<ILayerPropertyKeyframe> _keyframes = null!;
    private IDisposable _keyframeSubscription = null!;
    [Notify] private bool _hasChildren;
    [Notify] private bool _isExpanded;
    [Notify] private bool _isVisible;

    public PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory, IPropertyInputService propertyInputService)
    {
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _propertyInputService = propertyInputService;
        LayerPropertyGroup = layerPropertyGroup;
        TreeGroupViewModel = layerPropertyVmFactory.TreeGroupViewModel(this);
        TimelineGroupViewModel = layerPropertyVmFactory.TimelineGroupViewModel(this);

        LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;
        _isVisible = !LayerPropertyGroup.IsHidden;

        PopulateChildren();
    }

    public PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory, IPropertyInputService propertyInputService, BaseLayerBrush layerBrush)
    {
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _propertyInputService = propertyInputService;
        LayerBrush = layerBrush;
        LayerPropertyGroup = layerPropertyGroup;
        TreeGroupViewModel = layerPropertyVmFactory.TreeGroupViewModel(this);
        TimelineGroupViewModel = layerPropertyVmFactory.TimelineGroupViewModel(this);

        LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;
        _isVisible = !LayerPropertyGroup.IsHidden;

        PopulateChildren();
    }

    public PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory, IPropertyInputService propertyInputService, BaseLayerEffect layerEffect)
    {
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _propertyInputService = propertyInputService;
        LayerEffect = layerEffect;
        LayerPropertyGroup = layerPropertyGroup;
        TreeGroupViewModel = layerPropertyVmFactory.TreeGroupViewModel(this);
        TimelineGroupViewModel = layerPropertyVmFactory.TimelineGroupViewModel(this);

        LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;
        _isVisible = !LayerPropertyGroup.IsHidden;

        PopulateChildren();
    }

    public ObservableCollection<PropertyViewModelBase> Children { get; private set; } = null!;
    public LayerPropertyGroup LayerPropertyGroup { get; }
    public BaseLayerBrush? LayerBrush { get; }
    public BaseLayerEffect? LayerEffect { get; }

    public TreeGroupViewModel TreeGroupViewModel { get; }
    public TimelineGroupViewModel TimelineGroupViewModel { get; }
    
    public override ReadOnlyObservableCollection<ILayerPropertyKeyframe> Keyframes => _keyframes;

    public List<ILayerPropertyKeyframe> GetAllKeyframes(bool expandedOnly)
    {
        List<ILayerPropertyKeyframe> result = new();
        if (expandedOnly && !IsExpanded)
            return result;

        foreach (PropertyViewModelBase child in Children)
        {
            if (child is PropertyViewModel profileElementPropertyViewModel)
                result.AddRange(profileElementPropertyViewModel.TimelinePropertyViewModel.GetAllKeyframes());
            else if (child is PropertyGroupViewModel profileElementPropertyGroupViewModel)
                result.AddRange(profileElementPropertyGroupViewModel.GetAllKeyframes(expandedOnly));
        }

        return result;
    }

    public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels(bool expandedOnly)
    {
        List<ITimelineKeyframeViewModel> result = new();
        if (expandedOnly && !IsExpanded)
            return result;

        foreach (PropertyViewModelBase child in Children)
        {
            if (child is PropertyViewModel profileElementPropertyViewModel)
                result.AddRange(profileElementPropertyViewModel.TimelinePropertyViewModel.GetAllKeyframeViewModels());
            else if (child is PropertyGroupViewModel profileElementPropertyGroupViewModel)
                result.AddRange(profileElementPropertyGroupViewModel.GetAllKeyframeViewModels(expandedOnly));
        }

        return result;
    }

    private void PopulateChildren()
    {
        Children = new ObservableCollection<PropertyViewModelBase>();

        // Get all properties and property groups and create VMs for them
        // The group has methods for getting this without reflection but then we lose the order of the properties as they are defined on the group
        // Sorting is done to ensure properties defined by the Core (such as on layers) are always on top.
        foreach (PropertyInfo propertyInfo in LayerPropertyGroup.GetType().GetProperties().OrderBy(p => p.DeclaringType?.Assembly != Constants.CoreAssembly))
        {
            if (Attribute.IsDefined(propertyInfo, typeof(LayerPropertyIgnoreAttribute)))
                continue;

            if (typeof(ILayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
            {
                ILayerProperty? value = (ILayerProperty?) propertyInfo.GetValue(LayerPropertyGroup);
                // Ensure a supported input VM was found, otherwise don't add it
                if (value != null && _propertyInputService.CanCreatePropertyInputViewModel(value))
                    Children.Add(_layerPropertyVmFactory.PropertyViewModel(value));
            }
            else if (typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
            {
                LayerPropertyGroup? value = (LayerPropertyGroup?) propertyInfo.GetValue(LayerPropertyGroup);
                if (value != null)
                    Children.Add(_layerPropertyVmFactory.PropertyGroupViewModel(value));
            }
        }

        HasChildren = Children.Any(i => i is PropertyViewModel {IsVisible: true} or PropertyGroupViewModel {IsVisible: true});
        _keyframeSubscription = Children
            .ToObservableChangeSet()
            .TransformMany(c => c.Keyframes)
            .Bind(out ReadOnlyObservableCollection<ILayerPropertyKeyframe> keyframes)
            .Subscribe();

        _keyframes = keyframes;
    }

    private void LayerPropertyGroupOnVisibilityChanged(object? sender, EventArgs e)
    {
        IsVisible = !LayerPropertyGroup.IsHidden;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        LayerPropertyGroup.VisibilityChanged -= LayerPropertyGroupOnVisibilityChanged;
        while (Children.Any())
        {
            if (Children[0] is IDisposable disposable)
                disposable.Dispose();
            Children.RemoveAt(0);
        }
        _keyframeSubscription.Dispose();
    }
}