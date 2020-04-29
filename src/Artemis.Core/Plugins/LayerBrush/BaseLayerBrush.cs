using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    /// <summary>
    ///     A basic layer brush that does not implement any layer property, to use properties with persistent storage,
    ///     implement <see cref="LayerBrush{T}" /> instead
    /// </summary>
    public abstract class BaseLayerBrush : IDisposable
    {
        /// <summary>
        ///     Gets the layer this brush is applied to
        /// </summary>
        public Layer Layer { get; internal set; }

        /// <summary>
        ///     Gets the descriptor of this brush
        /// </summary>
        public LayerBrushDescriptor Descriptor { get; internal set; }

        /// <summary>
        ///     Gets the plugin info that defined this brush
        /// </summary>
        public PluginInfo PluginInfo => Descriptor.LayerBrushProvider.PluginInfo;

        internal virtual LayerPropertyGroup BaseProperties => null;

        /// <summary>
        ///     Called when the brush is being removed from the layer
        /// </summary>
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

        internal virtual void InitializeProperties(ILayerService layerService, string path)
        {
        }

        internal virtual void ApplyToEntity()
        {
        }

        internal virtual void UpdateProperties(double deltaTime)
        {
        }

        internal virtual void OverrideProperties(TimeSpan overrideTime)
        {
        }

        internal virtual IReadOnlyCollection<BaseLayerProperty> GetAllLayerProperties()
        {
            return new List<BaseLayerProperty>();
        }
    }
}