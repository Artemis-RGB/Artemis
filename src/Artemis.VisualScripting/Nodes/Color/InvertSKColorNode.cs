using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Invert Color", "Inverts a color by a specified amount in percent")]
    public class InvertSKColorNode : Node
    {
        public InvertSKColorNode() : base("Invert Color", "Inverts a color")
        {
            Input = CreateInputPin<SKColor>();
            Output = CreateOutputPin<SKColor>();
        }

        public InputPin<SKColor> Input { get; }
        public OutputPin<SKColor> Output { get; set; }

        public override void Evaluate()
        {
            Input.Value.ToHsl(out float h, out float s, out float l);
            h += 180;
            Output.Value = SKColor.FromHsl(h % 360, s, l);
        }
    }
}