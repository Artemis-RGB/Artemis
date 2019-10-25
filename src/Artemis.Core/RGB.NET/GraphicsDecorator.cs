using System;
using System.Drawing;
using System.Linq;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Rectangle = RGB.NET.Core.Rectangle;

namespace Artemis.Core.RGB.NET
{
    public class GraphicsDecorator : AbstractDecorator, IBrushDecorator, IDisposable
    {
        private readonly double _scale;
        private DirectBitmap _bitmap;

        public GraphicsDecorator(ILedGroup ledGroup, double scale)
        {
            _scale = scale;

            var leds = ledGroup.GetLeds().ToList();
            if (!leds.Any())
                _bitmap = null;
            else
            {
                var width = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.X + l.AbsoluteLedRectangle.Width) * scale, 4096);
                var height = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.Y + l.AbsoluteLedRectangle.Height) * scale, 4096);

                _bitmap = new DirectBitmap((int) width, (int) height);
            }
        }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            var x = renderTarget.Led.AbsoluteLedRectangle.Center.X * _scale;
            var y = renderTarget.Led.AbsoluteLedRectangle.Center.Y * _scale;
            if (_bitmap != null && _bitmap.Width - 1 >= x && _bitmap.Height - 1 >= y)
            {
                var pixel = _bitmap.GetPixel((int) x, (int) y);
                return new Color(pixel.A, pixel.R, pixel.G, pixel.B);
            }

            return new Color(0, 0, 0);
        }

        public override void OnDetached(IDecoratable decoratable)
        {
            Dispose();
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
            _bitmap = null;
        }

        public Graphics GetGraphics()
        {
            return _bitmap == null ? null : Graphics.FromImage(_bitmap.Bitmap);
        }

        public Bitmap GetBitmap()
        {
            return _bitmap?.Bitmap;
        }
    }
}