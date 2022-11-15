using Artemis.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Gradient Builder", "Generates a gradient based on some values", "Color", OutputType = typeof(ColorGradient), HelpUrl = "https://krazydad.com/tutorials/makecolors.php")]
    public class GradientBuilderNode : Node
    {
        public OutputPin<ColorGradient> Output { get; }

        public InputPin<Numeric> Frequency1 { get; }
        public InputPin<Numeric> Frequency2 { get; }
        public InputPin<Numeric> Frequency3 { get; }
        public InputPin<Numeric> Phase1 { get; }
        public InputPin<Numeric> Phase2 { get; }
        public InputPin<Numeric> Phase3 { get; }
        public InputPin<Numeric> Center { get; }
        public InputPin<Numeric> Width { get; }
        public InputPin<Numeric> Length { get; }

        public GradientBuilderNode()
        {
            Output = CreateOutputPin<ColorGradient>();
            Frequency1 = CreateInputPin<Numeric>("Frequency 1");
            Frequency2 = CreateInputPin<Numeric>("Frequency 2");
            Frequency3 = CreateInputPin<Numeric>("Frequency 3");
            Phase1 = CreateInputPin<Numeric>("Phase 1");
            Phase2 = CreateInputPin<Numeric>("Phase 2");
            Phase3 = CreateInputPin<Numeric>("Phase 3");
            Center = CreateInputPin<Numeric>("Center");
            Width = CreateInputPin<Numeric>("Width");
            Length = CreateInputPin<Numeric>("Length");
        }

        public override void Evaluate()
        {
            var gradient = new ColorGradient();

            for (int i = 0; i < Length.Value; i++)
            {
                var r = Math.Sin(Frequency1.Value * i + Phase1.Value) * Width.Value + Center.Value;
                var g = Math.Sin(Frequency2.Value * i + Phase2.Value) * Width.Value + Center.Value;
                var b = Math.Sin(Frequency3.Value * i + Phase3.Value) * Width.Value + Center.Value;
                gradient.Add(new ColorGradientStop(new SKColor((byte)r, (byte)g, (byte)b), i / Length.Value));
            }

            Output.Value = gradient;
        }
    }
}
