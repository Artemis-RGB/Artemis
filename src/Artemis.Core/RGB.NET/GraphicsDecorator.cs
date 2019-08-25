using System;
using System.Drawing;
using System.Linq;
using RGB.NET.Core;
using RGB.NET.Groups;
using Color = RGB.NET.Core.Color;
using Rectangle = RGB.NET.Core.Rectangle;

namespace Artemis.Core.RGB.NET
{
    public class GraphicsDecorator : AbstractDecorator, IBrushDecorator
    {
        private readonly DirectBitmap _bitmap;

        public GraphicsDecorator(ListLedGroup ledGroup)
        {
            var width = ledGroup.GetLeds().Max(l => l.AbsoluteLedRectangle.X + l.AbsoluteLedRectangle.Width);
            var height = ledGroup.GetLeds().Max(l => l.AbsoluteLedRectangle.Y + l.AbsoluteLedRectangle.Height);

            _bitmap = new DirectBitmap((int) width, (int) height);
        }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            var point = renderTarget.Point;
            if (_bitmap.Width - 1 >= point.X && _bitmap.Height - 1 >= point.Y)
            {
                var pixel = _bitmap.GetPixel((int) point.X, (int) point.Y);
                return new Color(pixel.A, pixel.R, pixel.G, pixel.B);
            }

            return new Color(0, 0, 0);
        }

        public Graphics GetGraphics()
        {
            return Graphics.FromImage(_bitmap.Bitmap);
        }
    }
}