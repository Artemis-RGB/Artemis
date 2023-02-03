using Artemis.Core;
using Artemis.Core.ColorScience;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Sorted Gradient", "Generates a sorted gradient from the given colors", "Color", InputType = typeof(SKColor), OutputType = typeof(ColorGradient))]
    public class SortedGradientNode : Node
    {
        private int lastComputedColorGroup;
        public InputPinCollection<SKColor> Inputs { get; }
        public OutputPin<ColorGradient> Output { get; }

        public SortedGradientNode()
        {
            Inputs = CreateInputPinCollection<SKColor>();
            Output = CreateOutputPin<ColorGradient>();
            lastComputedColorGroup = 0;
        }

        public override void Evaluate()
        {
            int newHash = GetInputColorHash();
            if (newHash == lastComputedColorGroup)
                return;
            
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
            lastComputedColorGroup = newHash;
        }
        
        private int GetInputColorHash()
        {
            int hash = 0;
            
            foreach (SKColor color in Inputs.Values)
                hash = HashCode.Combine(hash, color.GetHashCode());

            return hash;
        }
    }
}
