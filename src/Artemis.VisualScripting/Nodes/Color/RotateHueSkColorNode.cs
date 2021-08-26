using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Rotate Color Hue", "Rotates the hue of a color by a specified amount in degrees", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
    public class RotateHueSKColorNode : Node
    {
        public RotateHueSKColorNode() : base("Rotate Color Hue", "Rotates the hue of a color by a specified amount in degrees")
        {
            Input = CreateInputPin<SKColor>("Color");
            Amount = CreateInputPin<float>("Amount");
            Output = CreateOutputPin<SKColor>();
        }

        public InputPin<SKColor> Input { get; }
        public InputPin<float> Amount { get; }
        public OutputPin<SKColor> Output { get; set; }

        public override void Evaluate()
        {
            Input.Value.ToHsl(out float h, out float s, out float l);
            h += Amount.Value;
            Output.Value = SKColor.FromHsl(h % 360, s, l);
        }
    }
}