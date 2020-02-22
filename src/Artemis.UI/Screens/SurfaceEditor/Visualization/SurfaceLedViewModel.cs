using System.ComponentModel;
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

            Led.PropertyChanged += ApplyViewModelOnLedChange;
        }

        public Led Led { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public void ApplyLedToViewModel()
        {
            // Don't want ActualLocation here since rotation is done in XAML
            X = Led.Location.X * Led.Device.Scale.Horizontal;
            Y = Led.Location.Y * Led.Device.Scale.Vertical;
            Width = Led.ActualSize.Width;
            Height = Led.ActualSize.Height;
        }

        private void ApplyViewModelOnLedChange(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Location" || args.PropertyName == "ActualSize") ApplyLedToViewModel();
        }
    }
}