using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrushes.Internal;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrushes
{
    public abstract class PerLedLayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        protected PerLedLayerBrush()
        {
            BrushType = LayerBrushType.Regular;
        }


        /// <summary>
        ///     The main method of rendering for this type of brush. Called once per frame for each LED in the layer
        ///     <para>
        ///         Note: Due to transformations, the render point may not match the position of the LED, always use the render
        ///         point to determine where the color will go.
        ///     </para>
        /// </summary>
        /// <param name="led">The LED that will receive the color</param>
        /// <param name="renderPoint">The point at which the color is located</param>
        /// <returns>The color the LED will receive</returns>
        public abstract SKColor GetColor(ArtemisLed led, SKPoint renderPoint);

        internal override void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            // We don't want translations on this canvas because that'll displace the LEDs, translations are applied to the points of each LED instead
            Layer.ExcludeCanvasFromTranslation(canvas, true);

            if (Layer.General.ResizeMode == LayerResizeMode.Normal)
            {
                // Apply a translated version of the shape as the clipping mask
                var shapePath = new SKPath(Layer.LayerShape.Path);
                Layer.IncludePathInTranslation(shapePath, true);
                canvas.ClipPath(shapePath);
            }

            using var pointsPath = new SKPath();
            using var ledPaint = new SKPaint();
            foreach (var artemisLed in Layer.Leds)
            {
                pointsPath.AddPoly(new[]
                {
                    new SKPoint(0, 0),
                    new SKPoint(artemisLed.AbsoluteRenderRectangle.Left - Layer.Bounds.Left, artemisLed.AbsoluteRenderRectangle.Top - Layer.Bounds.Top)
                });
            }

            Layer.ExcludePathFromTranslation(pointsPath, true);
            var points = pointsPath.Points;
            for (var index = 0; index < Layer.Leds.Count; index++)
            {
                var artemisLed = Layer.Leds[index];
                var renderPoint = points[index * 2 + 1];
                if (!float.IsFinite(renderPoint.X) || !float.IsFinite(renderPoint.Y))
                    continue;

                // Let the brush determine the color
                ledPaint.Color = GetColor(artemisLed, renderPoint);

                var ledRectangle = SKRect.Create(
                    artemisLed.AbsoluteRenderRectangle.Left - Layer.Bounds.Left,
                    artemisLed.AbsoluteRenderRectangle.Top - Layer.Bounds.Top,
                    artemisLed.AbsoluteRenderRectangle.Width,
                    artemisLed.AbsoluteRenderRectangle.Height
                );

                canvas.DrawRect(ledRectangle, ledPaint);
            }
        }

        internal override void Initialize(IRenderElementService renderElementService)
        {
            InitializeProperties(renderElementService);
        }
    }
}