using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model registration
    /// </summary>
    public class DataModelRegistration
    {
        internal DataModelRegistration(DataModel dataModel, PluginImplementation pluginImplementation)
        {
            DataModel = dataModel;
            PluginImplementation = pluginImplementation;

            PluginImplementation.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the data model that has been registered
        /// </summary>
        public DataModel DataModel { get; }

        /// <summary>
        ///     Gets the plugin the data model is associated with
        /// </summary>
        public PluginImplementation PluginImplementation { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the registration is in the internal Core store
        /// </summary>
        public bool IsInStore { get; internal set; }

        private void OnDisabled(object sender, EventArgs e)
        {
            PluginImplementation.Disabled -= OnDisabled;
            if (IsInStore)
                DataModelStore.Remove(this);
        }
    }
}