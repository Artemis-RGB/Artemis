using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.SurfaceEditor
{
    public class SurfaceLedViewModel : PropertyChangedBase
    {
        public SurfaceLedViewModel(Led led)
        {
            Led = led;
            ApplyLedToViewModel();
        }

        public void ApplyLedToViewModel()
        {
            X = Led.LedRectangle.X;
            Y = Led.LedRectangle.Y;
            Width = Led.LedRectangle.Width;
            Height = Led.LedRectangle.Height;
        }

        public Led Led { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}