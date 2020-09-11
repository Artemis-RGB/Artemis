using System;
using System.Reflection;
using Ninject.Extensions.Factory;

namespace Artemis.UI.Ninject.InstanceProviders
{
    public class DataBindingsViewModelInstanceProvider : StandardInstanceProvider
    {
        protected override Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            return base.GetType(methodInfo, arguments);
        }
    }
}