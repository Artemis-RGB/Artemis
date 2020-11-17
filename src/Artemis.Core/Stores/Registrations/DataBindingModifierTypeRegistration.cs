using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model registration
    /// </summary>
    public class DataBindingModifierTypeRegistration
    {
        internal DataBindingModifierTypeRegistration(BaseDataBindingModifierType dataBindingModifierType, Plugin plugin)
        {
            DataBindingModifierType = dataBindingModifierType;
            Plugin = plugin;

            Plugin.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the data binding modifier that has been registered
        /// </summary>
        public BaseDataBindingModifierType DataBindingModifierType { get; }

        /// <summary>
        ///     Gets the plugin the data binding modifier is associated with
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the registration is in the internal Core store
        /// </summary>
        public bool IsInStore { get; internal set; }

        private void OnDisabled(object? sender, EventArgs e)
        {
            Plugin.Disabled -= OnDisabled;
            if (IsInStore)
                DataBindingModifierTypeStore.Remove(this);
        }
    }
}