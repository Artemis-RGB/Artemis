using System.Collections.ObjectModel;
using Artemis.Core.Annotations;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Services.Interfaces
{
    public interface IDataModelService : IArtemisService
    {
        ReadOnlyCollection<DataModel> DataModelExpansions { get; }

        /// <summary>
        ///     Add an expansion to the datamodel to be available for use after the next update
        /// </summary>
        /// <param name="baseDataModelExpansion"></param>
        void AddExpansion(DataModel baseDataModelExpansion);

        /// <summary>
        ///     Remove a previously added expansion so that it is no longer available and updated
        /// </summary>
        /// <param name="baseDataModelExpansion"></param>
        void RemoveExpansion(DataModel baseDataModelExpansion);

        /// <summary>
        ///     If found, returns the data model of the provided plugin
        /// </summary>
        /// <param name="plugin">Should be a module with a data model or a data model expansion</param>
        DataModel GetPluginDataModel(Plugin plugin);

        /// <summary>
        ///     Registers a new condition operator for use in layer conditions
        /// </summary>
        /// <param name="pluginInfo">The PluginInfo of the plugin this condition operator belongs to</param>
        /// <param name="layerConditionOperator">The condition operator to register</param>
        void RegisterConditionOperator([NotNull] PluginInfo pluginInfo, [NotNull] LayerConditionOperator layerConditionOperator);

        /// <summary>
        ///     Removes a condition operator so it is no longer available for use in layer conditions
        /// </summary>
        /// <param name="layerConditionOperator">The layer condition operator to remove</param>
        void RemoveConditionOperator([NotNull] LayerConditionOperator layerConditionOperator);
    }
}