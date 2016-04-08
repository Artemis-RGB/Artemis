using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Artemis.Models.Profiles
{
    [XmlInclude(typeof (LinearGradientBrush))]
    [XmlInclude(typeof (RadialGradientBrush))]
    [XmlInclude(typeof (MatrixTransform))]
    public class LayerPropertiesModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Opacity { get; set; }
        public bool ContainedBrush { get; set; }
        public LayerAnimation Animation { get; set; }
        public double AnimationSpeed { get; set; }
        public Brush Brush { get; set; }
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