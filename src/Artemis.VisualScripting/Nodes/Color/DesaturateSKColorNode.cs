using System;
using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Desaturate Color", "Desaturates a color by a specified amount in percent")]
    public class DesaturateSKColorNode : Node
    {
        public DesaturateSKColorNode() : base("Desaturate Color", "Desaturates a color by a specified amount in percent")
        {
            Input = CreateInputPin<SKColor>("Color");
            Percentage = CreateInputPin<float>("%");
            Output = CreateOutputPin<SKColor>();
        }

        public InputPin<SKColor> Input { get; }
        public InputPin<float> Percentage { get; }
        public OutputPin<SKColor> Output { get; set; }

        public override void Evaluate()
        {
            Input.Value.ToHsl(out float h, out float s, out float l);
            s -= Percentage.Value;
            s = Math.Clamp(s, 0, 100);
            Output.Value = SKColor.FromHsl(h, s, l);
        }
    }
}