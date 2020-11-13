using System;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Input;

namespace Artemis.UI.Shared.Ninject.Factories
{
    public interface ISharedVmFactory
    {
    }

    public interface IDataModelVmFactory : ISharedVmFactory
    {
        DataModelDynamicViewModel DataModelDynamicViewModel(Module module);
        DataModelStaticViewModel DataModelStaticViewModel(Type targetType, DataModelPropertyAttribute targetDescription);
    }
}