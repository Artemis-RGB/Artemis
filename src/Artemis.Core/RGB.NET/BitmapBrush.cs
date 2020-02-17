using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Models;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.RGB.NET
{
    public class BitmapBrush : AbstractDecoratable<IBrushDecorator>, IBrush, IDisposable
    {
        private readonly PluginSetting<int> _sampleSizeSetting;

        #region Constructors

        public BitmapBrush(Scale scale, PluginSetting<int> sampleSizeSetting)
        {
            _sampleSizeSetting = sampleSizeSetting;
            Scale = scale;
        }

        #endregion

        #region Properties & Fields

        /// <inheritdoc />
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc />
        public BrushCalculationMode BrushCalculationMode { get; set; } = BrushCalculationMode.Absolute;

        /// <inheritdoc />
        public double Brightness { get; set; }

        /// <inheritdoc />
        public double Opacity { get; set; }

        /// <inheritdoc />
        public IList<IColorCorrection> ColorCorrections { get; } = new List<IColorCorrection>();

        /// <inheritdoc />
        public Rectangle RenderedRectangle { get; private set; }

        /// <inheritdoc />
        public Dictionary<BrushRenderTarget, Color> RenderedTargets { get; } = new Dictionary<BrushRenderTarget, Color>();

        public Scale Scale { get; set; }
        public Scale RenderedScale { get; private set; }
        public SKBitmap Bitmap { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual void PerformRender(Rectangle rectangle, IEnumerable<BrushRenderTarget> renderTargets)
        {
            if (RenderedRectangle != rectangle || RenderedScale != Scale)
                Bitmap = null;

            RenderedRectangle = rectangle;
            RenderedScale = Scale;
            RenderedTargets.Clear();

            if (Bitmap == null)
                CreateBitmap(RenderedRectangle);

            if (_sampleSizeSetting.Value == 1)
                TakeCenter(renderTargets);
            else
                TakeSamples(renderTargets);
        }

        private void TakeCenter(IEnumerable<BrushRenderTarget> renderTargets)
        {
            foreach (var renderTarget in renderTargets)
            {
                var scaledLocation = renderTarget.Point * Scale;
                if (scaledLocation.X < Bitmap.Width && scaledLocation.Y < Bitmap.Height)
                    RenderedTargets[renderTarget] = Bitmap.GetPixel(scaledLocation.X.RoundToInt(), scaledLocation.Y.RoundToInt()).ToRgbColor();
            }
        }

        private void TakeSamples(IEnumerable<BrushRenderTarget> renderTargets)
        {
            var sampleSize = _sampleSizeSetting.Value;
            var sampleDepth = Math.Sqrt(sampleSize).RoundToInt();

            var bitmapWidth = Bitmap.Width;
            var bitmapHeight = Bitmap.Height;

            var pixmap = Bitmap.PeekPixels();
            foreach (var renderTarget in renderTargets)
            {
                // SKRect has all the good stuff we need
                var left = (int) ((renderTarget.Rectangle.Location.X + 4) * Scale.Horizontal);
                var top = (int) ((renderTarget.Rectangle.Location.Y + 4) * Scale.Vertical);
                var width = (int) ((renderTarget.Rectangle.Size.Width - 8) * Scale.Horizontal);
                var height = (int) ((renderTarget.Rectangle.Size.Height - 8) * Scale.Vertical);

                var verticalSteps = height / (sampleDepth - 1);
                var horizontalSteps = width / (sampleDepth - 1);

                int a = 0, r = 0, g = 0, b = 0;
                for (var horizontalStep = 0; horizontalStep < sampleDepth; horizontalStep++)
                {
                    for (var verticalStep = 0; verticalStep < sampleDepth; verticalStep++)
                    {
                        var x = left + horizontalSteps * horizontalStep;
                        var y = top + verticalSteps * verticalStep;
                        if (x < 0 || x > bitmapWidth || y < 0 || y > bitmapHeight)
                            continue;

                        var color = pixmap.GetPixelColor(x, y);
                        a += color.Alpha;
                        r += color.Red;
                        g += color.Green;
                        b += color.Blue;

                        // Uncomment to view the sample pixels in the debugger, need a checkbox in the actual debugger but this was a quickie
                        // Bitmap.SetPixel(x, y, new SKColor(0, 255, 0));
                    }
                }

                RenderedTargets[renderTarget] = new Color(a / sampleSize, r / sampleSize, g / sampleSize, b / sampleSize);
            }
        }

        private void CreateBitmap(Rectangle rectangle)
        {
            var width = Math.Min((rectangle.Location.X + rectangle.Size.Width) * Scale.Horizontal, 4096);
            var height = Math.Min((rectangle.Location.Y + rectangle.Size.Height) * Scale.Vertical, 4096);
            Bitmap = new SKBitmap(new SKImageInfo(width.RoundToInt(), height.RoundToInt(), SKColorType.Rgb888x));
        }

        /// <inheritdoc />
        public virtual void PerformFinalize()
        {
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
        }

        #endregion
    }
}