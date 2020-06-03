using System;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using RGB.NET.Groups;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class RgbNetLayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        protected RgbNetLayerBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            BrushType = LayerBrushType.RgbNet;
            LedGroup = new ListLedGroup();

            Layer = layer;
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
        }

        /// <summary>
        ///     The LED group this layer brush is applied to
        /// </summary>
        public ListLedGroup LedGroup { get; internal set; }

        /// <summary>
        ///     Called when Artemis needs an instance of the RGB.NET brush you are implementing
        /// </summary>
        /// <returns>Your RGB.NET brush</returns>
        public abstract IBrush GetBrush();

        public sealed override void Dispose()
        {
            Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
            LedGroup.Detach();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

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

        internal override void Initialize(ILayerService layerService)
        {
            InitializeProperties(layerService);
            UpdateLedGroup();
        }

        // Not used in this brush type
        internal override void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            throw new NotImplementedException("RGB.NET layer brushes do not implement InternalRender");
        }

        internal override IBrush InternalGetBrush()
        {
            return GetBrush();
        }

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            UpdateLedGroup();
        }
    }
}