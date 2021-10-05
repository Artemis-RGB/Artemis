using System;
using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Saturate Color", "Saturates a color by a specified amount in percent", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
    public class SaturateSKColorNode : Node
    {
        public SaturateSKColorNode() : base("Saturate Color", "Saturates a color by a specified amount in percent")
        {
            Input = CreateInputPin<SKColor>("Color");
            Percentage = CreateInputPin<Numeric>("%");
            Output = CreateOutputPin<SKColor>();
        }

        public InputPin<SKColor> Input { get; }
        public InputPin<Numeric> Percentage { get; }
        public OutputPin<SKColor> Output { get; set; }

        public override void Evaluate()
        {
            Input.Value.ToHsl(out float h, out float s, out float l);
            s += s * (Percentage.Value / 100f);
            Output.Value = SKColor.FromHsl(h, Math.Clamp(s, 0f, 100f), l);
        }
    }
}