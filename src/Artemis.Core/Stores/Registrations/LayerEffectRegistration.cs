using System;
using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer effect registration
    /// </summary>
    public class LayerEffectRegistration
    {
        internal LayerEffectRegistration(LayerEffectDescriptor descriptor, PluginImplementation pluginImplementation)
        {
            LayerEffectDescriptor = descriptor;
            PluginImplementation = pluginImplementation;

            PluginImplementation.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the layer effect descriptor that has been registered
        /// </summary>
        public LayerEffectDescriptor LayerEffectDescriptor { get; }

        /// <summary>
        ///     Gets the plugin the layer effect is associated with
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
                LayerEffectStore.Remove(this);
        }
    }
}