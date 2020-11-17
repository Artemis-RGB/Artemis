using System;
using System.Collections.Generic;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     The RGB.NET brush Artemis uses to map the SkiaSharp bitmap to LEDs
    /// </summary>
    public sealed class BitmapBrush : AbstractDecoratable<IBrushDecorator>, IBrush, IDisposable
    {
        private readonly object _disposeLock;
        private readonly PluginSetting<int> _sampleSizeSetting;

        #region Constructors

        internal BitmapBrush(Scale scale, PluginSetting<int> sampleSizeSetting)
        {
            _sampleSizeSetting = sampleSizeSetting;
            Scale = scale;
            _disposeLock = new object();
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

        /// <summary>
        ///     Gets or sets the desired scale of the bitmap brush
        /// </summary>
        public Scale Scale { get; set; }

        /// <summary>
        ///     Gets the last rendered scale of the bitmap brush
        /// </summary>
        public Scale RenderedScale { get; private set; }

        /// <summary>
        ///     Gets the bitmap used to sample the brush
        /// </summary>
        public SKBitmap Bitmap { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void PerformRender(Rectangle rectangle, IEnumerable<BrushRenderTarget> renderTargets)
        {
            lock (_disposeLock)
            {
                // Can happen during surface change
                if (IsDisposed)
                    return;

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
        }

        private void TakeCenter(IEnumerable<BrushRenderTarget> renderTargets)
        {
            foreach (BrushRenderTarget renderTarget in renderTargets)
            {
                Point scaledLocation = renderTarget.Point * Scale;
                if (scaledLocation.X < Bitmap.Width && scaledLocation.Y < Bitmap.Height)
                    RenderedTargets[renderTarget] = Bitmap.GetPixel(scaledLocation.X.RoundToInt(), scaledLocation.Y.RoundToInt()).ToRgbColor();
            }
        }

        private void TakeSamples(IEnumerable<BrushRenderTarget> renderTargets)
        {
            int sampleSize = _sampleSizeSetting.Value;
            int sampleDepth = Math.Sqrt(sampleSize).RoundToInt();

            int bitmapWidth = Bitmap.Width;
            int bitmapHeight = Bitmap.Height;

            using SKPixmap pixmap = Bitmap.PeekPixels();
            foreach (BrushRenderTarget renderTarget in renderTargets)
            {
                // SKRect has all the good stuff we need
                int left = (int) ((renderTarget.Rectangle.Location.X + 4) * Scale.Horizontal);
                int top = (int) ((renderTarget.Rectangle.Location.Y + 4) * Scale.Vertical);
                int width = (int) ((renderTarget.Rectangle.Size.Width - 8) * Scale.Horizontal);
                int height = (int) ((renderTarget.Rectangle.Size.Height - 8) * Scale.Vertical);

                int verticalSteps = height / (sampleDepth - 1);
                int horizontalSteps = width / (sampleDepth - 1);

                int a = 0, r = 0, g = 0, b = 0;
                for (int horizontalStep = 0; horizontalStep < sampleDepth; horizontalStep++)
                {
                    for (int verticalStep = 0; verticalStep < sampleDepth; verticalStep++)
                    {
                        int x = left + horizontalSteps * horizontalStep;
                        int y = top + verticalSteps * verticalStep;
                        if (x < 0 || x >= bitmapWidth || y < 0 || y >= bitmapHeight)
                            continue;

                        SKColor color = pixmap.GetPixelColor(x, y);
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
            double width = Math.Min((rectangle.Location.X + rectangle.Size.Width) * Scale.Horizontal, 4096);
            double height = Math.Min((rectangle.Location.Y + rectangle.Size.Height) * Scale.Vertical, 4096);
            Bitmap = new SKBitmap(new SKImageInfo(width.RoundToInt(), height.RoundToInt(), SKColorType.Rgb888x));
        }

        /// <inheritdoc />
        public void PerformFinalize()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_disposeLock)
            {
                Bitmap?.Dispose();
                IsDisposed = true;
            }
        }

        internal bool IsDisposed { get; set; }

        #endregion
    }
}