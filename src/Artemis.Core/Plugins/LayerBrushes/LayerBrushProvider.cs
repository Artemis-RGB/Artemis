using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     Allows you to create one or more <see cref="LayerBrush{T}" />s usable by profile layers.
    /// </summary>
    public abstract class LayerBrushProvider : Plugin
    {
        private readonly List<LayerBrushDescriptor> _layerBrushDescriptors;

        /// <summary>
        ///     Allows you to register one or more <see cref="LayerBrush{T}" />s usable by profile layers.
        /// </summary>
        protected LayerBrushProvider()
        {
            _layerBrushDescriptors = new List<LayerBrushDescriptor>();
            PluginDisabled += OnPluginDisabled;
        }

        /// <summary>
        ///     A read-only collection of all layer brushes added with <see cref="RegisterLayerBrushDescriptor{T}" />
        /// </summary>
        public ReadOnlyCollection<LayerBrushDescriptor> LayerBrushDescriptors => _layerBrushDescriptors.AsReadOnly();

        /// <summary>
        ///     Registers a layer brush descriptor for a given layer brush, so that it appears in the UI.
        ///     <para>Note: You do not need to manually remove these on disable</para>
        /// </summary>
        /// <typeparam name="T">The type of the layer brush you wish to register</typeparam>
        /// <param name="displayName">The name to display in the UI</param>
        /// <param name="description">The description to display in the UI</param>
        /// <param name="icon">
        ///     The Material icon to display in the UI, a full reference can be found
        ///     <see href="https://materialdesignicons.com">here</see>
        /// </param>
        protected void RegisterLayerBrushDescriptor<T>(string displayName, string description, string icon) where T : BaseLayerBrush
        {
            if (!Enabled)
                throw new ArtemisPluginException(PluginInfo, "Can only add a layer brush descriptor when the plugin is enabled");

            _layerBrushDescriptors.Add(new LayerBrushDescriptor(displayName, description, icon, typeof(T), this));
        }

        private void OnPluginDisabled(object sender, EventArgs e)
        {
            _layerBrushDescriptors.Clear();
        }
    }
}