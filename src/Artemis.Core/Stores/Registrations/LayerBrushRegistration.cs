using System;
using Artemis.Core.LayerBrushes;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer brush registration
    /// </summary>
    public class LayerBrushRegistration
    {
        internal LayerBrushRegistration(LayerBrushDescriptor descriptor, Plugin plugin)
        {
            LayerBrushDescriptor = descriptor;
            Plugin = plugin;

            Plugin.PluginDisabled += PluginOnPluginDisabled;
        }

        /// <summary>
        ///     Gets the layer brush descriptor that has been registered
        /// </summary>
        public LayerBrushDescriptor LayerBrushDescriptor { get; }

        /// <summary>
        ///     Gets the plugin the layer brush is associated with
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
                LayerBrushStore.Remove(this);
        }
    }
}