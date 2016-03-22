using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Artemis.Models.Profiles
{
    public class LayerPropertiesModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Opacity { get; set; }
        public bool ContainedBrush { get; set; }
        public LinearGradientMode GradientMode { get; set; }
        public bool Rotate { get; set; }
        public double RotateSpeed { get; set; }
        public List<Color> Colors { get; set; }

        public LayerPropertiesModel()
        {
            Colors = new List<Color>();
        }
    }
}