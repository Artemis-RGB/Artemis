using Artemis.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Sorted Gradient", "Generates a sorted gradient from the given colors", "Color", InputType = typeof(SKColor), OutputType = typeof(ColorGradient))]
    public class SortedGradientNode : Node
    {
        public InputPinCollection<SKColor> Inputs { get; }
        public OutputPin<ColorGradient> Output { get; }

        public SortedGradientNode()
        {
            Inputs = CreateInputPinCollection<SKColor>();
            Output = CreateOutputPin<ColorGradient>();
        }

        public override void Evaluate()
        {
            var colors = Inputs.Values.ToArray();

            if (colors.Length == 0)
            {
                Output.Value = null;
                return;
            }

            ColorUtilities.Sort(colors, SKColors.White);

            var gradient = new ColorGradient();
            for (int i = 0; i < colors.Length; i++)
            {
                gradient.Add(new(colors[i], (float)i / (colors.Length - 1)));
            }

            Output.Value = gradient;
        }
    }
}
