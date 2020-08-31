using System;
using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Properties;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    public interface IDataModelService : IArtemisService
    {
        /// <summary>
        ///     Gets a read-only collection of all registered condition operators
        /// </summary>
        IReadOnlyCollection<DisplayConditionOperator> RegisteredConditionOperators { get; }

        /// <summary>
        ///     Gets a read-only collection of all registered data model expansions
        /// </summary>
        IReadOnlyCollection<DataModel> DataModelExpansions { get; }

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
        ///     If found, returns the data model of the provided plugin
        /// </summary>
        /// <param name="pluginGuid">Should be a module with a data model or a data model expansion</param>
        DataModel GetPluginDataModelByGuid(Guid pluginGuid);

        /// <summary>
        ///     Determines whether the given plugin expands the main data model
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        bool GetPluginExtendsDataModel(Plugin plugin);

        /// <summary>
        ///     Registers a new condition operator for use in layer conditions
        /// </summary>
        /// <param name="pluginInfo">The PluginInfo of the plugin this condition operator belongs to</param>
        /// <param name="displayConditionOperator">The condition operator to register</param>
        void RegisterConditionOperator([NotNull] PluginInfo pluginInfo, [NotNull] DisplayConditionOperator displayConditionOperator);

        /// <summary>
        ///     Removes a condition operator so it is no longer available for use in layer conditions
        /// </summary>
        /// <param name="displayConditionOperator">The layer condition operator to remove</param>
        void RemoveConditionOperator([NotNull] DisplayConditionOperator displayConditionOperator);

        /// <summary>
        ///     Returns all the display condition operators compatible with the provided type
        /// </summary>
        List<DisplayConditionOperator> GetCompatibleConditionOperators(Type type);

        /// <summary>
        ///     Gets a condition operator by its plugin GUID and type name
        /// </summary>
        /// <param name="operatorPluginGuid">The operator's plugin GUID</param>
        /// <param name="operatorType">The type name of the operator</param>
        DisplayConditionOperator GetConditionOperator(Guid operatorPluginGuid, string operatorType);

        /// <summary>
        ///     Logs a predicate deserialization failure
        /// </summary>
        /// <param name="displayConditionPredicate">The predicate that failed to deserialize</param>
        /// <param name="exception">The JSON exception that occurred</param>
        void LogPredicateDeserializationFailure(DisplayConditionPredicate displayConditionPredicate, JsonException exception);

        /// <summary>
        ///     Logs a list predicate deserialization failure
        /// </summary>
        /// <param name="displayConditionListPredicate">The list predicate that failed to deserialize</param>
        /// <param name="exception">The JSON exception that occurred</param>
        void LogListPredicateDeserializationFailure(DisplayConditionListPredicate displayConditionListPredicate, JsonException exception);
    }
}