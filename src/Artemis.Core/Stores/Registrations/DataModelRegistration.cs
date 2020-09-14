using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model registration
    /// </summary>
    public class DataModelRegistration
    {
        internal DataModelRegistration(DataModel dataModel, Plugin plugin)
        {
            DataModel = dataModel;
            Plugin = plugin;

            Plugin.PluginDisabled += PluginOnPluginDisabled;
        }

        /// <summary>
        ///     Gets the data model that has been registered
        /// </summary>
        public DataModel DataModel { get; }

        /// <summary>
        ///     Gets the plugin the data model is associated with
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
                DataModelStore.Remove(this);
        }
    }
}