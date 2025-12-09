using System;
using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using DryIoc;

namespace Artemis.UI.DryIoc.InstanceProviders;

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
            throw new ArtemisUIException("Could not find the LayerProperty type");

        Type? genericType = typeof(TimelinePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());
        return (ITimelinePropertyViewModel)_container.Resolve(genericType, new object[] { layerProperty, propertyViewModel });
    }

    public ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel)
    {
        // Find LayerProperty type
        Type? layerPropertyType = layerProperty.GetType();
        while (layerPropertyType != null && (!layerPropertyType.IsGenericType || layerPropertyType.GetGenericTypeDefinition() != typeof(LayerProperty<>)))
            layerPropertyType = layerPropertyType.BaseType;
        if (layerPropertyType == null)
            throw new ArtemisUIException("Could not find the LayerProperty type");

        Type? genericType = typeof(TreePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());

        return (ITreePropertyViewModel)_container.Resolve(genericType, new object[] { layerProperty, propertyViewModel });
    }
}