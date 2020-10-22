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

        public ConditionOperatorRegistration RegisterConditionOperator(PluginInfo pluginInfo, BaseConditionOperator conditionOperator)
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

        public List<BaseConditionOperator> GetConditionOperatorsForType(Type type, ConditionParameterSide side)
        {
            return ConditionOperatorStore.GetForType(type, side).Select(r => r.ConditionOperator).ToList();
        }

        public BaseConditionOperator GetConditionOperator(Guid operatorPluginGuid, string operatorType)
        {
            return ConditionOperatorStore.Get(operatorPluginGuid, operatorType)?.ConditionOperator;
        }

        private void RegisterBuiltInConditionOperators()
        {
            // General usage for any type
            RegisterConditionOperator(Constants.CorePluginInfo, new EqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new NotEqualConditionOperator());

            // Numeric operators
            RegisterConditionOperator(Constants.CorePluginInfo, new NumberEqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new NumberNotEqualConditionOperator());
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
            RegisterConditionOperator(Constants.CorePluginInfo, new StringMatchesRegexConditionOperator());

            // Null checks, at the bottom
            // TODO: Implement a priority mechanism
            RegisterConditionOperator(Constants.CorePluginInfo, new NullConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new NotNullConditionOperator());
        }
    }
}