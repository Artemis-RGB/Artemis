using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.LayerElement
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create one or more <see cref="LayerElement" />s usable by profile layers.
    /// </summary>
    public abstract class LayerElementProvider : Plugin
    {
        private readonly List<LayerElementDescriptor> _layerElementDescriptors;

        protected LayerElementProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            _layerElementDescriptors = new List<LayerElementDescriptor>();
        }

        public ReadOnlyCollection<LayerElementDescriptor> LayerElementDescriptors => _layerElementDescriptors.AsReadOnly();

        protected void AddLayerElementDescriptor<T>(string displayName, string description, string icon) where T : LayerElement
        {
            _layerElementDescriptors.Add(new LayerElementDescriptor(displayName, description, icon, typeof(T), this));
        }
    }
}