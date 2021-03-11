using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;
using Ninject;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     An RGB.NET brush that uses RGB.NET's per-LED rendering engine.
    ///     <para>Note: This brush type always renders on top of regular brushes</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RgbNetLayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RgbNetLayerBrush{T}" /> class
        /// </summary>
        protected RgbNetLayerBrush()
        {
            BrushType = LayerBrushType.RgbNet;
            SupportsTransformation = false;
        }

        /// <summary>
        ///     The LED group this layer effect is applied to
        /// </summary>
        public ListLedGroup? LedGroup { get; internal set; }

        /// <summary>
        ///     For internal use only, is public for dependency injection but ignore pl0x
        /// </summary>
        [Inject]
        public IRgbService? RgbService { get; set; }

        /// <summary>
        ///     Called when Artemis needs an instance of the RGB.NET effect you are implementing
        /// </summary>
        /// <returns>Your RGB.NET effect</returns>
        public abstract IBrush GetBrush();

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (RgbService == null)
                    throw new ArtemisCoreException("Cannot dispose RGB.NET layer brush because RgbService is not set");

                Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
                LedGroup?.Detach();
                LedGroup = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        internal void UpdateLedGroup()
        {
            if (LedGroup == null)
                return;

            if (Layer.Parent != null)
                LedGroup.ZIndex = Layer.Parent.Children.Count - Layer.Parent.Children.IndexOf(Layer);
            else
                LedGroup.ZIndex = 1;

            List<Led> missingLeds = Layer.Leds.Where(l => !LedGroup.ContainsLed(l.RgbLed)).Select(l => l.RgbLed).ToList();
            List<Led> extraLeds = LedGroup.Where(l => Layer.Leds.All(layerLed => layerLed.RgbLed != l)).ToList();
            LedGroup.AddLeds(missingLeds);
            LedGroup.RemoveLeds(extraLeds);
            LedGroup.Brush = GetBrush();
        }

        internal override void Initialize()
        {
            if (RgbService == null)
                throw new ArtemisCoreException("Cannot initialize RGB.NET layer brush because RgbService is not set");

            LedGroup = new ListLedGroup(RgbService.Surface);
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;

            InitializeProperties();
            UpdateLedGroup();
        }

        // Not used in this effect type
        internal override void InternalRender(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
            throw new NotImplementedException("RGB.NET layer effects do not implement InternalRender");
        }

        private void LayerOnRenderPropertiesUpdated(object? sender, EventArgs e)
        {
            UpdateLedGroup();
        }
    }
}