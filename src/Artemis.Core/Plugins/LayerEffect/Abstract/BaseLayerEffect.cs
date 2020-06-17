using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Plugins.LayerEffect.Abstract
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerEffect" /> instead
    /// </summary>
    public abstract class BaseLayerEffect : PropertyChangedBase, IDisposable
    {
        /// <summary>
        ///     Gets the unique ID of this effect
        /// </summary>
        public Guid EntityId { get; internal set; }

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this effect is applied to
        /// </summary>
        public EffectProfileElement ProfileElement { get; internal set; }

        /// <summary>
        ///     The name which appears in the editor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets whether the effect has been renamed by the user, if true consider refraining from changing the name
        ///     programatically
        /// </summary>
        public bool HasBeenRenamed { get; set; }

        /// <summary>
        ///     Gets the order in which this effect appears in the update loop and editor
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     Gets the descriptor of this effect
        /// </summary>
        public LayerEffectDescriptor Descriptor { get; internal set; }

        /// <summary>
        ///     Gets the plugin info that defined this effect
        /// </summary>
        public PluginInfo PluginInfo => Descriptor.LayerEffectProvider.PluginInfo;

        /// <summary>
        ///     Gets a reference to the layer property group without knowing it's type
        /// </summary>
        public virtual LayerPropertyGroup BaseProperties => null;

        internal string PropertyRootPath => $"LayerEffect.{EntityId}.{GetType().Name}.";

        public void Dispose()
        {
            DisableLayerEffect();
        }

        /// <summary>
        ///     Called when the layer brush is activated
        /// </summary>
        public abstract void EnableLayerEffect();

        /// <summary>
        ///     Called when the layer brush is deactivated
        /// </summary>
        public abstract void DisableLayerEffect();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Called before the layer or folder will be rendered
        /// </summary>
        /// <param name="canvas">The canvas used to render the frame</param>
        /// <param name="canvasInfo">Info on the canvas size and pixel type</param>
        /// <param name="renderBounds">The bounds this layer/folder will render in</param>
        /// <param name="paint">The paint this layer/folder will use to render</param>
        public abstract void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint);

        /// <summary>
        ///     Called after the layer of folder has been rendered
        /// </summary>
        /// <param name="canvas">The canvas used to render the frame</param>
        /// <param name="canvasInfo">Info on the canvas size and pixel type</param>
        /// <param name="renderBounds">The bounds this layer/folder rendered in</param>
        /// <param name="paint">The paint this layer/folder used to render</param>
        public abstract void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint);

        /// <summary>
        ///     Allows you to return a clip that is applied to the layer shape (or in case of a folder, all layer shapes in the
        ///     folder).
        ///     <para>
        ///         To avoid breaking other effects, use this instead of applying a clip to the entire canvas if you want to limit
        ///         where shapes may render
        ///     </para>
        /// </summary>
        /// <param name="renderBounds">The bounds this layer/folder rendered in</param>
        /// <returns></returns>
        public virtual SKPath CreateShapeClip(SKPath renderBounds)
        {
            return new SKPath();
        }

        internal void InternalPreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            var x = path.Bounds.Left;
            var y = path.Bounds.Top;
            // Move the canvas to the top-left of the render path
            canvas.Translate(x, y);

            // Pass the render path to the layer effect positioned at 0,0
            path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));

            PreProcess(canvas, canvasInfo, path, paint);

            // Move the canvas back
            canvas.Translate(x * -1, y * -1);
        }

        internal void InternalPostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            var x = path.Bounds.Left;
            var y = path.Bounds.Top;
            // Move the canvas to the top-left of the render path
            canvas.Translate(x, y);

            // Pass the render path to the layer effect positioned at 0,0
            path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));

            PostProcess(canvas, canvasInfo, path, paint);

            // Move the canvas back
            canvas.Translate(x * -1, y * -1);
        }

        internal virtual SKPath InternalCreateShapeClip(SKPath path)
        {
            var x = path.Bounds.Left;
            var y = path.Bounds.Top;

            path.Transform(SKMatrix.MakeTranslation(x * -1, y * -1));
            var shapeClip = CreateShapeClip(path);
            if (!shapeClip.IsEmpty)
                shapeClip.Transform(SKMatrix.MakeTranslation(x, y));
            return shapeClip;
        }

        // Not only is this needed to initialize properties on the layer effects, it also prevents implementing anything
        // but LayerEffect<T> outside the core
        internal abstract void Initialize(ILayerService layerService);
    }
}