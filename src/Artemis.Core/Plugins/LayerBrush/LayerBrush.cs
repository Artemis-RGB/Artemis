using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrush : IDisposable
    {
        protected LayerBrush(Layer layer, LayerBrushDescriptor descriptor)
        {
            Layer = layer;
            Descriptor = descriptor;
        }

        public Layer Layer { get; }
        public LayerBrushDescriptor Descriptor { get; }

        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(double deltaTime)
        {
        }

        /// <summary>
        ///     The main method of rendering anything to the layer. The provided <see cref="SKCanvas" /> is specific to the layer
        ///     and matches it's width and height.
        ///     <para>Called during rendering, in the order configured on the layer</para>
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="path">The path to be filled, represents the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public virtual void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
        }

        /// <summary>
        ///     Provides an easy way to add your own properties to the layer, for more info see <see cref="LayerProperty{T}" />.
        ///     <para>Note: If found, the last value and keyframes are loaded and set when calling this method.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">The parent of this property, use this to create a tree-hierarchy in the editor</param>
        /// <param name="id">A and ID identifying your property, must be unique within your plugin</param>
        /// <param name="name">A name for your property, this is visible in the editor</param>
        /// <param name="description">A description for your property, this is visible in the editor</param>
        /// <returns>The layer property</returns>
        protected LayerProperty<T> RegisterLayerProperty<T>(BaseLayerProperty parent, string id, string name, string description)
        {
            var property = new LayerProperty<T>(Layer, Descriptor.LayerBrushProvider.PluginInfo, parent, id, name, description);
            Layer.RegisterLayerProperty(property);
            return property;
        }

        /// <summary>
        ///     Provides an easy way to add your own properties to the layer, for more info see <see cref="LayerProperty{T}" />.
        ///     <para>Note: If found, the last value and keyframes are loaded and set when calling this method.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">A and ID identifying your property, must be unique within your plugin</param>
        /// <param name="name">A name for your property, this is visible in the editor</param>
        /// <param name="description">A description for your property, this is visible in the editor</param>
        /// <returns>The layer property</returns>
        protected LayerProperty<T> RegisterLayerProperty<T>(string id, string name, string description)
        {
            var property = new LayerProperty<T>(Layer, Descriptor.LayerBrushProvider.PluginInfo, Layer.BrushReferenceProperty.Parent, id, name, description);
            Layer.RegisterLayerProperty(property);
            return property;
        }
    }
}