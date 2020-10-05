using System;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings;
using Ninject.Extensions.Factory;

namespace Artemis.UI.Ninject.InstanceProviders
{
    public class DataBindingsViewModelInstanceProvider : StandardInstanceProvider
    {
        protected override Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            if (methodInfo.ReturnType != typeof(IDataBindingViewModel))
                return base.GetType(methodInfo, arguments);

            // Find LayerProperty type
            Type descriptionPropertyType = arguments[0].GetType();
            while (descriptionPropertyType != null && (!descriptionPropertyType.IsGenericType || descriptionPropertyType.GetGenericTypeDefinition() != typeof(DataBindingRegistration<,>)))
                descriptionPropertyType = descriptionPropertyType.BaseType;
            if (descriptionPropertyType == null)
                return base.GetType(methodInfo, arguments);

            return typeof(DataBindingViewModel<,>).MakeGenericType(descriptionPropertyType.GetGenericArguments());
        }
    }
}