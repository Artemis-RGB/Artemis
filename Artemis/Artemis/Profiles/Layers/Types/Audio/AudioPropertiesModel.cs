using System.ComponentModel;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesModel : LayerPropertiesModel
    {
        public AudioPropertiesModel(LayerPropertiesModel properties) : base(properties)
        {
        }

        public int Sensitivity { get; set; }
        public double FadeSpeed { get; set; }
        public Direction Direction { get; set; }
    }

    public enum Direction
    {
        [Description("Top to bottom")] TopToBottom,
        [Description("Bottom to top")] BottomToTop,
        [Description("Left to right")] LeftToRight,
        [Description("Right to left")] RightToLeft
    }

    public enum ScalingStrategy
    {
        [Description("Decibel")] Decibel,
        [Description("Linear")] Linear,
        [Description("Square root")] Sqrt
    }
}