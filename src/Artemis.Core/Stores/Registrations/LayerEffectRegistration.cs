using System;
using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer effect registration
    /// </summary>
    public class LayerEffectRegistration
    {
        internal LayerEffectRegistration(LayerEffectDescriptor descriptor, Plugin plugin)
        {
            LayerEffectDescriptor = descriptor;
            Plugin = plugin;

            Plugin.PluginDisabled += PluginOnPluginDisabled;
        }

        /// <summary>
        ///     Gets the layer effect descriptor that has been registered
        /// </summary>
        public LayerEffectDescriptor LayerEffectDescriptor { get; }

        /// <summary>
        ///     Gets the plugin the layer effect is associated with
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
                LayerEffectStore.Remove(this);
        }
    }
}