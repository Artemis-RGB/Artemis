using System;
using System.Linq;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.RGB.NET
{
    public class GraphicsDecorator : AbstractDecorator, IBrushDecorator, IDisposable
    {
        private readonly double _scale;

        public GraphicsDecorator(ILedGroup ledGroup, double scale)
        {
            _scale = scale;

            var leds = ledGroup.GetLeds().ToList();
            if (!leds.Any())
                return;

            var width = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.Location.X + l.AbsoluteLedRectangle.Size.Width) * scale, 4096);
            var height = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.Location.Y + l.AbsoluteLedRectangle.Size.Height) * scale, 4096);
            Bitmap = new SKBitmap(new SKImageInfo(RoundToInt(width), RoundToInt(height)));
            Canvas = new SKCanvas(Bitmap);
        }

        public SKBitmap Bitmap { get; private set; }
        public SKCanvas Canvas { get; private set; }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            if (Bitmap == null)
                return new Color(0, 0, 0);

            var x = renderTarget.Led.AbsoluteLedRectangle.Center.X * _scale;
            var y = renderTarget.Led.AbsoluteLedRectangle.Center.Y * _scale;

            var pixel = Bitmap.GetPixel(RoundToInt(x), RoundToInt(y));
            return new Color(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
        }

        public override void OnDetached(IDecoratable decoratable)
        {
            Dispose();
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
            Canvas?.Dispose();
            Bitmap = null;
            Canvas = null;
        }

        private int RoundToInt(double number)
        {
            return (int) Math.Round(number, MidpointRounding.AwayFromZero);
        }
    }
}