using System;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrush : IDisposable
    {
        private ILayerService _layerService;

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
        ///     <para>Called during rendering or layer preview, in the order configured on the layer</para>
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="canvasInfo"></param>
        /// <param name="path">The path to be filled, represents the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public virtual void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
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
        /// <param name="defaultValue">The default value of the property, if not configured by the user</param>
        /// <returns>The layer property</returns>
        protected LayerProperty<T> RegisterLayerProperty<T>(BaseLayerProperty parent, string id, string name, string description, T defaultValue = default)
        {
            var property = new LayerProperty<T>(Layer, Descriptor.LayerBrushProvider.PluginInfo, parent, id, name, description) {Value = defaultValue};
            Layer.Properties.RegisterLayerProperty(property);
            // It's fine if this is null, it'll be picked up by SetLayerService later
            _layerService?.InstantiateKeyframeEngine(property);
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
        /// <param name="defaultValue">The default value of the property, if not configured by the user</param>
        /// <returns>The layer property</returns>
        protected LayerProperty<T> RegisterLayerProperty<T>(string id, string name, string description, T defaultValue = default)
        {
            // Check if the property already exists
            var existing = Layer.Properties.FirstOrDefault(p =>
                p.PluginInfo.Guid == Descriptor.LayerBrushProvider.PluginInfo.Guid &&
                p.Id == id &&
                p.Name == name &&
                p.Description == description);

            if (existing != null)
            {
                // If it exists and the types match, return the existing property
                if (existing.Type == typeof(T))
                    return (LayerProperty<T>) existing;
                // If it exists and the types are different, something is wrong
                throw new ArtemisPluginException($"Cannot register the property {id} with different types twice.");
            }

            var property = new LayerProperty<T>(Layer, Descriptor.LayerBrushProvider.PluginInfo, Layer.Properties.BrushReference.Parent, id, name, description)
            {
                Value = defaultValue
            };

            Layer.Properties.RegisterLayerProperty(property);
            // It's fine if this is null, it'll be picked up by SetLayerService later
            _layerService?.InstantiateKeyframeEngine(property);
            return property;
        }

        /// <summary>
        /// Allows you to remove layer properties previously added by using  <see cref="RegisterLayerProperty{T}(BaseLayerProperty,string,string,string,T)" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layerProperty"></param>
        protected void UnRegisterLayerProperty<T>(LayerProperty<T> layerProperty)
        {
            if (layerProperty == null)
                return;

            if (Layer.Properties.Any(p => p == layerProperty))
                Layer.Properties.RemoveLayerProperty(layerProperty);
        }

        internal void SetLayerService(ILayerService layerService)
        {
            _layerService = layerService;
            foreach (var baseLayerProperty in Layer.Properties)
                _layerService.InstantiateKeyframeEngine(baseLayerProperty);
        }
    }
}