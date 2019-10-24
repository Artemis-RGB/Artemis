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
        private DirectBitmap _bitmap;

        public GraphicsDecorator(ILedGroup ledGroup)
        {
            var leds = ledGroup.GetLeds().ToList();
            if (!leds.Any())
                _bitmap = null;
            else
            {
                var width = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.X + l.AbsoluteLedRectangle.Width), 2000);
                var height = Math.Min(leds.Max(l => l.AbsoluteLedRectangle.Y + l.AbsoluteLedRectangle.Height), 2000);

                _bitmap = new DirectBitmap((int) width, (int) height);
            }
        }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            var point = renderTarget.Led.AbsoluteLedRectangle.Center;
            if (_bitmap != null && _bitmap.Width - 1 >= point.X && _bitmap.Height - 1 >= point.Y)
            {
                var pixel = _bitmap.GetPixel((int) point.X, (int) point.Y);
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