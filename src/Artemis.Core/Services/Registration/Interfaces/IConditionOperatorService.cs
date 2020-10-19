using System;
using System.Collections.Generic;
using Artemis.Core.Properties;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to register and retrieve conditions operators used by display conditions
    /// </summary>
    public interface IConditionOperatorService : IArtemisService
    {
        /// <summary>
        ///     Registers a new condition operator for use in layer conditions
        /// </summary>
        /// <param name="pluginInfo">The PluginInfo of the plugin this condition operator belongs to</param>
        /// <param name="conditionOperator">The condition operator to register</param>
        ConditionOperatorRegistration RegisterConditionOperator([NotNull] PluginInfo pluginInfo, [NotNull] BaseConditionOperator conditionOperator);

        /// <summary>
        ///     Removes a condition operator so it is no longer available for use in layer conditions
        /// </summary>
        /// <param name="registration">The registration of the condition operator to remove</param>
        void RemoveConditionOperator([NotNull] ConditionOperatorRegistration registration);

        /// <summary>
        ///     Returns all the condition operators compatible with the provided type
        /// </summary>
        List<BaseConditionOperator> GetConditionOperatorsForType(Type type, ConditionParameterSide side);

        /// <summary>
        ///     Gets a condition operator by its plugin GUID and type name
        /// </summary>
        /// <param name="operatorPluginGuid">The operator's plugin GUID</param>
        /// <param name="operatorType">The type name of the operator</param>
        BaseConditionOperator GetConditionOperator(Guid operatorPluginGuid, string operatorType);
    }
}