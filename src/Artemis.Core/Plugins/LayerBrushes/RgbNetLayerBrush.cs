using System;
using System.Linq;
using Artemis.Core.Services;
using RGB.NET.Core;
using RGB.NET.Groups;
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
        public ListLedGroup LedGroup { get; internal set; }

        /// <summary>
        ///     Called when Artemis needs an instance of the RGB.NET effect you are implementing
        /// </summary>
        /// <returns>Your RGB.NET effect</returns>
        public abstract IBrush GetBrush();

        internal void UpdateLedGroup()
        {
            // TODO: This simply renders it on top of the rest, get a ZIndex based on layer position
            LedGroup.ZIndex = 1;

            var missingLeds = Layer.Leds.Where(l => !LedGroup.ContainsLed(l.RgbLed)).Select(l => l.RgbLed).ToList();
            var extraLeds = LedGroup.GetLeds().Where(l => Layer.Leds.All(layerLed => layerLed.RgbLed != l)).ToList();
            LedGroup.AddLeds(missingLeds);
            LedGroup.RemoveLeds(extraLeds);
            LedGroup.Brush = GetBrush();
        }

        internal override void Initialize()
        {
            LedGroup = new ListLedGroup();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;

            InitializeProperties();
            UpdateLedGroup();
        }

        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
                LedGroup.Detach();
            }

            base.Dispose(disposing);
        }

        // Not used in this effect type
        internal override void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            throw new NotImplementedException("RGB.NET layer effectes do not implement InternalRender");
        }

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            UpdateLedGroup();
        }
    }
}