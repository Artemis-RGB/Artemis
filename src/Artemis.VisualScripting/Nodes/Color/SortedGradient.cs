using Artemis.Core;
using Artemis.Core.ColorScience;
using SkiaSharp;

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
            SKColor[] colors = Inputs.Values.ToArray();

            if (colors.Length == 0)
            {
                Output.Value = null;
                return;
            }

            ColorSorter.Sort(colors, SKColors.Black);

            ColorGradient gradient = new();
            for (int i = 0; i < colors.Length; i++)
            {
                gradient.Add(new ColorGradientStop(colors[i], (float)i / (colors.Length - 1)));
            }

            Output.Value = gradient;
        }
    }
}
