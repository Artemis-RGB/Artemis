using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core.Services
{
    internal class ConditionOperatorService : IConditionOperatorService
    {
        public ConditionOperatorService()
        {
            RegisterBuiltInConditionOperators();
        }

        public ConditionOperatorRegistration RegisterConditionOperator(Plugin plugin, BaseConditionOperator conditionOperator)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (conditionOperator == null)
                throw new ArgumentNullException(nameof(conditionOperator));

            conditionOperator.Plugin = plugin;
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

        public BaseConditionOperator? GetConditionOperator(Guid operatorPluginGuid, string operatorType)
        {
            return ConditionOperatorStore.Get(operatorPluginGuid, operatorType)?.ConditionOperator;
        }

        private void RegisterBuiltInConditionOperators()
        {
            // General usage for any type
            RegisterConditionOperator(Constants.CorePlugin, new EqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new NotEqualConditionOperator());

            // Numeric operators
            RegisterConditionOperator(Constants.CorePlugin, new NumberEqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new NumberNotEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new LessThanConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new GreaterThanConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new LessThanOrEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new GreaterThanOrEqualConditionOperator());

            // String operators
            RegisterConditionOperator(Constants.CorePlugin, new StringEqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringNotEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringNotContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringStartsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringEndsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringMatchesRegexConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringNullConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new StringNotNullConditionOperator());

            // Null checks, at the bottom
            // TODO: Implement a priority mechanism
            RegisterConditionOperator(Constants.CorePlugin, new NullConditionOperator());
            RegisterConditionOperator(Constants.CorePlugin, new NotNullConditionOperator());
        }
    }
}