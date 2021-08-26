using System;
using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Desaturate Color", "Desaturates a color by a specified amount in percent", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
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
            s -= s * (Percentage.Value / 100f);
            Output.Value = SKColor.FromHsl(h, Math.Clamp(s, 0f, 100f), l);
        }
    }
}