using System;
using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a factory for view models provided by the Artemis Shared UI library
    /// </summary>
    public interface ISharedVmFactory
    {
    }

    /// <summary>
    ///     A factory that allows the creation of data model view models
    /// </summary>
    public interface IDataModelVmFactory : ISharedVmFactory
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelDynamicViewModel" /> class
        /// </summary>
        /// <param name="modules">The modules to associate the dynamic view model with</param>
        /// <returns>A new instance of the <see cref="DataModelDynamicViewModel" /> class</returns>
        DataModelDynamicViewModel DataModelDynamicViewModel(List<Module> modules);

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelStaticViewModel" /> class
        /// </summary>
        /// <param name="targetType">The type of property that is expected in this input</param>
        /// <param name="targetDescription">The description of the property that this input is for</param>
        /// <returns>A new instance of the <see cref="DataModelStaticViewModel" /> class</returns>
        DataModelStaticViewModel DataModelStaticViewModel(Type targetType, DataModelPropertyAttribute targetDescription);
    }
}