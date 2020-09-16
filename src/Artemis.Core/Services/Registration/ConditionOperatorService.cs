using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DefaultTypes;

namespace Artemis.Core.Services
{
    internal class ConditionOperatorService : IConditionOperatorService
    {
        public ConditionOperatorService()
        {
            RegisterBuiltInConditionOperators();
        }

        public ConditionOperatorRegistration RegisterConditionOperator(PluginInfo pluginInfo, ConditionOperator conditionOperator)
        {
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (conditionOperator == null)
                throw new ArgumentNullException(nameof(conditionOperator));

            conditionOperator.PluginInfo = pluginInfo;
            return ConditionOperatorStore.Add(conditionOperator);
        }

        public void RemoveConditionOperator(ConditionOperatorRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            ConditionOperatorStore.Remove(registration);
        }

        public List<ConditionOperator> GetConditionOperatorsForType(Type type)
        {
            return ConditionOperatorStore.GetForType(type).Select(r => r.ConditionOperator).ToList();
        }

        public ConditionOperator GetConditionOperator(Guid operatorPluginGuid, string operatorType)
        {
            return ConditionOperatorStore.Get(operatorPluginGuid, operatorType)?.ConditionOperator;
        }

        private void RegisterBuiltInConditionOperators()
        {
            // General usage for any type
            RegisterConditionOperator(Constants.CorePluginInfo, new EqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new NotEqualConditionOperator());

            // Numeric operators
            RegisterConditionOperator(Constants.CorePluginInfo, new LessThanConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new GreaterThanConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new LessThanOrEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new GreaterThanOrEqualConditionOperator());

            // String operators
            RegisterConditionOperator(Constants.CorePluginInfo, new StringEqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNotEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNotContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringStartsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringEndsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNullConditionOperator());
        }
    }
}