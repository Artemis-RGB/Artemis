using SkiaSharp;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script bound to a specific <see cref="Layer" /> processed by a <see cref="ScriptingProvider" />.
    /// </summary>
    public abstract class LayerScript : Script
    {
        /// <summary>
        ///     Called whenever the layer is about to update
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnLayerUpdating(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the layer has been updated
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnLayerUpdated(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the layer is about to render
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="bounds">The area to be filled, covers the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public virtual void OnLayerRendering(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }

        /// <summary>
        ///     Called whenever the layer has been rendered
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="bounds">The area to be filled, covers the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public virtual void OnLayerRendered(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }
    }
}