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
        public LayerColorMode ColorMode { get; set; }
        public bool Rotate { get; set; }
        public double RotateSpeed { get; set; }
        public Brush Brush { get; set; }
    }

    public enum LayerColorMode
    {
        [Description("Gradient")] Gradient,
        [Description("Moving gradient")] MovingGradient,
        [Description("Shift")] Shift,
        [Description("Pulse")] Pulse
    }
}