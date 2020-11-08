using SkiaSharp;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     Represents a brush that renders on a per-layer basis
    /// </summary>
    /// <typeparam name="T">The type of brush properties</typeparam>
    public abstract class PerLedLayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PerLedLayerBrush{T}" /> class
        /// </summary>
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

        internal override void InternalRender(SKCanvas canvas, SKPath path, SKPaint paint)
        {
            // We don't want rotation on this canvas because that'll displace the LEDs, translations are applied to the points of each LED instead
            if (Layer.General.TransformMode.CurrentValue == LayerTransformMode.Normal && SupportsTransformation) 
                canvas.SetMatrix(canvas.TotalMatrix.PreConcat(Layer.GetTransformMatrix(true, false, false, true).Invert()));

            using SKPath pointsPath = new SKPath();
            using SKPaint ledPaint = new SKPaint();
            foreach (ArtemisLed artemisLed in Layer.Leds)
            {
                pointsPath.AddPoly(new[]
                {
                    new SKPoint(0, 0),
                    new SKPoint(artemisLed.AbsoluteRenderRectangle.Left - Layer.Bounds.Left, artemisLed.AbsoluteRenderRectangle.Top - Layer.Bounds.Top)
                });
            }

            // Apply the translation to the points of each LED instead
            if (Layer.General.TransformMode.CurrentValue == LayerTransformMode.Normal && SupportsTransformation) 
                pointsPath.Transform(Layer.GetTransformMatrix(true, true, true, true).Invert());

            SKPoint[] points = pointsPath.Points;
            for (int index = 0; index < Layer.Leds.Count; index++)
            {
                ArtemisLed artemisLed = Layer.Leds[index];
                SKPoint renderPoint = points[index * 2 + 1];
                if (!float.IsFinite(renderPoint.X) || !float.IsFinite(renderPoint.Y))
                    continue;

                // Let the brush determine the color
                ledPaint.Color = GetColor(artemisLed, renderPoint);

                SKRect ledRectangle = SKRect.Create(
                    artemisLed.AbsoluteRenderRectangle.Left - Layer.Bounds.Left,
                    artemisLed.AbsoluteRenderRectangle.Top - Layer.Bounds.Top,
                    artemisLed.AbsoluteRenderRectangle.Width,
                    artemisLed.AbsoluteRenderRectangle.Height
                );

                canvas.DrawRect(ledRectangle, ledPaint);
            }
        }

        internal override void Initialize()
        {
            InitializeProperties();
        }
    }
}