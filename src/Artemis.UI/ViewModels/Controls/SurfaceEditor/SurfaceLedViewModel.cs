using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.SurfaceEditor
{
    public class SurfaceLedViewModel : PropertyChangedBase
    {
        public SurfaceLedViewModel(Led led)
        {
            Led = led;

            X = Led.LedRectangle.X;
            Y = Led.LedRectangle.Y;
            Width = Led.LedRectangle.Width;
            Height = Led.LedRectangle.Height;
        }

        public Led Led { get; }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
    }
}