using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model registration
    /// </summary>
    public class ConditionOperatorRegistration
    {
        internal ConditionOperatorRegistration(BaseConditionOperator conditionOperator, Plugin plugin)
        {
            ConditionOperator = conditionOperator;
            Plugin = plugin;

            Plugin.PluginDisabled += PluginOnPluginDisabled;
        }

        /// <summary>
        ///     Gets the condition operator that has been registered
        /// </summary>
        public BaseConditionOperator ConditionOperator { get; }

        /// <summary>
        ///     Gets the plugin the condition operator is associated with
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the registration is in the internal Core store
        /// </summary>
        public bool IsInStore { get; internal set; }

        private void PluginOnPluginDisabled(object sender, EventArgs e)
        {
            Plugin.PluginDisabled -= PluginOnPluginDisabled;
            if (IsInStore)
                ConditionOperatorStore.Remove(this);
        }
    }
}