using System.ComponentModel;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesModel : LayerPropertiesModel
    {
        public AudioPropertiesModel(LayerPropertiesModel properties) : base(properties)
        {
        }

        [DefaultValue(MmDeviceType.Ouput)]
        public MmDeviceType DeviceType { get; set; }

        [DefaultValue("Default")]
        public string Device { get; set; }

        [DefaultValue(Direction.BottomToTop)]
        public Direction Direction { get; set; }

        [DefaultValue(ScalingStrategy.Decibel)]
        public ScalingStrategy ScalingStrategy { get; set; }
    }

    public enum MmDeviceType
    {
        [Description("Ouput")] Ouput,
        [Description("Input")] Input
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