using System;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a layer brush registration returned by calling <see cref="ILayerBrushService.RegisterLayerBrush"/>
    /// </summary>
    public class LayerBrushRegistration
    {
        internal LayerBrushRegistration(LayerBrushDescriptor descriptor, PluginFeature pluginFeature)
        {
            LayerBrushDescriptor = descriptor;
            PluginFeature = pluginFeature;

            PluginFeature.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the layer brush descriptor that has been registered
        /// </summary>
        public LayerBrushDescriptor LayerBrushDescriptor { get; }

        /// <summary>
        ///     Gets the plugin the layer brush is associated with
        /// </summary>
        public PluginFeature PluginFeature { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the registration is in the internal Core store
        /// </summary>
        public bool IsInStore { get; internal set; }

        private void OnDisabled(object? sender, EventArgs e)
        {
            PluginFeature.Disabled -= OnDisabled;
            if (IsInStore)
                LayerBrushStore.Remove(this);
        }
    }
}