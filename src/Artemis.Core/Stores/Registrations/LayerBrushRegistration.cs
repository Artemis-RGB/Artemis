using System;
using Artemis.Core.LayerBrushes;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer brush registration
    /// </summary>
    public class LayerBrushRegistration
    {
        internal LayerBrushRegistration(LayerBrushDescriptor descriptor, PluginImplementation pluginImplementation)
        {
            LayerBrushDescriptor = descriptor;
            PluginImplementation = pluginImplementation;

            PluginImplementation.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the layer brush descriptor that has been registered
        /// </summary>
        public LayerBrushDescriptor LayerBrushDescriptor { get; }

        /// <summary>
        ///     Gets the plugin the layer brush is associated with
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
                LayerBrushStore.Remove(this);
        }
    }
}