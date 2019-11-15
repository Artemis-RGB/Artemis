using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Visualization
{
    public class SurfaceLedViewModel : PropertyChangedBase
    {
        public SurfaceLedViewModel(Led led)
        {
            Led = led;
            ApplyLedToViewModel();
        }

        public Led Led { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public void ApplyLedToViewModel()
        {
            X = Led.LedRectangle.X;
            Y = Led.LedRectangle.Y;
            Width = Led.LedRectangle.Width;
            Height = Led.LedRectangle.Height;
        }
    }
}