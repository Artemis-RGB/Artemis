using System;
using Ninject;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     A class that describes a layer brush
    /// </summary>
    public class LayerBrushDescriptor
    {
        internal LayerBrushDescriptor(string displayName, string description, string icon, Type layerBrushType, LayerBrushProvider provider)
        {
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            LayerBrushType = layerBrushType;
            Provider = provider;
        }

        /// <summary>
        ///     The name that is displayed in the UI
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     The description that is displayed in the UI
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The Material icon to display in the UI, a full reference can be found
        ///     <see href="https://materialdesignicons.com">here</see>
        /// </summary>
        public string Icon { get; }

        /// <summary>
        ///     The type of the layer brush
        /// </summary>
        public Type LayerBrushType { get; }

        /// <summary>
        ///     The plugin that provided this <see cref="LayerBrushDescriptor" />
        /// </summary>
        public LayerBrushProvider Provider { get; }

        /// <summary>
        ///     Determines whether the provided <paramref name="reference" /> references to a brush provided by this descriptor
        /// </summary>
        public bool MatchesLayerBrushReference(LayerBrushReference? reference)
        {
            if (reference == null)
                return false;

            return Provider.Id == reference.LayerBrushProviderId && LayerBrushType.Name == reference.BrushType;
        }

        /// <summary>
        ///     Creates an instance of the described brush and applies it to the layer
        /// </summary>
        internal void CreateInstance(Layer layer)
        {
            if (layer == null) throw new ArgumentNullException(nameof(layer));
            if (layer.LayerBrush != null)
                throw new ArtemisCoreException("Layer already has an instantiated layer brush");

            BaseLayerBrush brush = (BaseLayerBrush) Provider.Plugin.Kernel!.Get(LayerBrushType);
            brush.Layer = layer;
            brush.Descriptor = this;
            brush.Initialize();
            brush.Update(0);

            layer.LayerBrush = brush;
            layer.OnLayerBrushUpdated();
        }
    }
}