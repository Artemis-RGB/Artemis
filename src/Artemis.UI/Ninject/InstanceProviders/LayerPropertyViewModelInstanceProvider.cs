using System;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;

namespace Artemis.UI.Ninject.InstanceProviders;

public class PropertyVmFactory : IPropertyVmFactory
{
    public ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel)
    {
        // Find LayerProperty type
        Type? layerPropertyType = layerProperty.GetType();
        while (layerPropertyType != null && (!layerPropertyType.IsGenericType || layerPropertyType.GetGenericTypeDefinition() != typeof(LayerProperty<>)))
            layerPropertyType = layerPropertyType.BaseType;
        if (layerPropertyType == null)
            return null;

        var genericType = typeof(TimelinePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());
        return (ITimelinePropertyViewModel)Activator.CreateInstance(genericType);
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
        return (ITreePropertyViewModel)Activator.CreateInstance(genericType);
    }
}