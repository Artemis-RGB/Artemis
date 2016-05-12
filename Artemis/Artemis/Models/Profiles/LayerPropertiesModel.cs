using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Artemis.Models.Profiles
{
    [XmlInclude(typeof(SolidColorBrush))]
    [XmlInclude(typeof(LinearGradientBrush))]
    [XmlInclude(typeof(RadialGradientBrush))]
    [XmlInclude(typeof(MatrixTransform))]
    public class LayerPropertiesModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Opacity { get; set; }
        public bool ContainedBrush { get; set; }
        public LayerAnimation Animation { get; set; }
        public double AnimationSpeed { get; set; }
        public Brush Brush { get; set; }

        public Rect GetRect(int scale = 4)
        {
            return new Rect(X*scale, Y*scale, Width*scale, Height*scale);
        }
    }


    public enum LayerAnimation
    {
        [Description("None")] None,
        [Description("Slide left")] SlideLeft,
        [Description("Slide right")] SlideRight,
        [Description("Slide up")] SlideUp,
        [Description("Slide down")] SlideDown,
        [Description("Grow")] Grow,
        [Description("Pulse")] Pulse
    }
}