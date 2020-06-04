using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.LayerBrush
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create one or more <see cref="LayerBrush" />s usable by profile layers.
    /// </summary>
    public abstract class LayerBrushProvider : Plugin
    {
        private readonly List<LayerBrushDescriptor> _layerBrushDescriptors;

        protected LayerBrushProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            _layerBrushDescriptors = new List<LayerBrushDescriptor>();
        }

        public ReadOnlyCollection<LayerBrushDescriptor> LayerBrushDescriptors => _layerBrushDescriptors.AsReadOnly();

        protected void AddLayerBrushDescriptor<T>(string displayName, string description, string icon) where T : BaseLayerBrush
        {
            _layerBrushDescriptors.Add(new LayerBrushDescriptor(displayName, description, icon, typeof(T), this));
        }
    }
}