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
            var pixel = _bitmap.GetPixel((int) (renderTarget.Rectangle.X + renderTarget.Rectangle.Width / 2), (int) (renderTarget.Rectangle.Y + renderTarget.Rectangle.Height / 2));
            return new Color(pixel.A, pixel.R, pixel.G, pixel.B);
        }

        public Graphics GetGraphics()
        {
            return Graphics.FromImage(_bitmap.Bitmap);
        }
    }
}