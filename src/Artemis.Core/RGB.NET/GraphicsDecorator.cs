using System.Drawing;
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
//            var width = ledGroup.GetLeds().Max(l => l.LedRectangle.X + l.LedRectangle.Width);
//            var height = ledGroup.GetLeds().Max(l => l.LedRectangle.Y + l.LedRectangle.Height);
            var width = 500;
            var height = 500;
            _bitmap = new DirectBitmap(width, height);
        }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
        {
            var pixel = _bitmap.GetPixel((int) (renderTarget.Rectangle.X + renderTarget.Rectangle.Width / 2), (int) (renderTarget.Rectangle.Y + renderTarget.Rectangle.Height / 2));
            return new Color(pixel.A, pixel.R, pixel.G, pixel.B);
        }

        public Graphics GetGraphics()
        {
            return Graphics.FromImage(_bitmap.Bitmap);
        }
    }
}