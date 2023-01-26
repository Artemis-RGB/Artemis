using System;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using DryIoc;

namespace Artemis.UI.Ninject.InstanceProviders;

public class PropertyVmFactory : IPropertyVmFactory
{
    private readonly IContainer _container;

    public PropertyVmFactory(IContainer container)
    {
        _container = container;
    }

    public ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel)
    {
        // Find LayerProperty type
        Type? layerPropertyType = layerProperty.GetType();
        while (layerPropertyType != null && (!layerPropertyType.IsGenericType || layerPropertyType.GetGenericTypeDefinition() != typeof(LayerProperty<>)))
            layerPropertyType = layerPropertyType.BaseType;
        if (layerPropertyType == null)
            return null;

        var genericType = typeof(TimelinePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());
        return _container.Resolve(genericType) as ITimelinePropertyViewModel;
    }

    public ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel)
    {
        // Find LayerProperty type
        Type? layerPropertyType = layerProperty.GetType();
        while (layerPropertyType != null && (!layerPropertyType.IsGenericType || layerPropertyType.GetGenericTypeDefinition() != typeof(LayerProperty<>)))
            layerPropertyType = layerPropertyType.BaseType;
        if (layerPropertyType == null)
            return null;

        var genericType = typeof(TreePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());

        return _container.Resolve(genericType) as ITreePropertyViewModel;
    }
}