using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model registration
    /// </summary>
    public class DataBindingModifierTypeRegistration
    {
        internal DataBindingModifierTypeRegistration(DataBindingModifierType dataBindingModifierType, Plugin plugin)
        {
            DataBindingModifierType = dataBindingModifierType;
            Plugin = plugin;

            Plugin.PluginDisabled += PluginOnPluginDisabled;
        }

        /// <summary>
        ///     Gets the data binding modifier that has been registered
        /// </summary>
        public DataBindingModifierType DataBindingModifierType { get; }

        /// <summary>
        ///     Gets the plugin the data binding modifier is associated with
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
                DataBindingModifierTypeStore.Remove(this);
        }
    }
}