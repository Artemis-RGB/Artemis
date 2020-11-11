using System;
using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to register and retrieve data models
    /// </summary>
    public interface IDataModelService : IArtemisService
    {
        /// <summary>
        ///     Add a data model to so that it is available to conditions and data bindings
        /// </summary>
        /// <param name="dataModel"></param>
        DataModelRegistration RegisterDataModel(DataModel dataModel);

        /// <summary>
        ///     Remove a previously added data model so that it is no longer available
        /// </summary>
        void RemoveDataModel(DataModelRegistration registration);

        /// <summary>
        ///     Returns a list of all registered data models
        /// </summary>
        List<DataModel> GetDataModels();

        /// <summary>
        ///     If found, returns the registered data model of type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">The type of the data model to find</typeparam>
        T GetDataModel<T>() where T : DataModel;

        /// <summary>
        ///     If found, returns the data model of the provided plugin
        /// </summary>
        /// <param name="pluginFeature">The plugin to find the data model of</param>
        DataModel? GetPluginDataModel(PluginFeature pluginFeature);
    }
}