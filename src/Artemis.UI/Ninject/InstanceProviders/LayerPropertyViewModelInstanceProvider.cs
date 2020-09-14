using System;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Ninject.Extensions.Factory;

namespace Artemis.UI.Ninject.InstanceProviders
{
    public class LayerPropertyViewModelInstanceProvider : StandardInstanceProvider
    {
        protected override Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            if (methodInfo.ReturnType != typeof(ITreePropertyViewModel) && methodInfo.ReturnType != typeof(ITimelinePropertyViewModel))
                return base.GetType(methodInfo, arguments);

            // Find LayerProperty type
            var layerPropertyType = arguments[0].GetType();
            while (layerPropertyType != null && (!layerPropertyType.IsGenericType || layerPropertyType.GetGenericTypeDefinition() != typeof(LayerProperty<>)))
                layerPropertyType = layerPropertyType.BaseType;
            if (layerPropertyType == null)
                return base.GetType(methodInfo, arguments);

            if (methodInfo.ReturnType == typeof(ITreePropertyViewModel))
                return typeof(TreePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());
            if (methodInfo.ReturnType == typeof(ITimelinePropertyViewModel))
                return typeof(TimelinePropertyViewModel<>).MakeGenericType(layerPropertyType.GetGenericArguments());

            return base.GetType(methodInfo, arguments);
        }
    }
}