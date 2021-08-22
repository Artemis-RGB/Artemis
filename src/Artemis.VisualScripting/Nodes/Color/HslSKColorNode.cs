using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("HSL Color", "Creates a color from hue, saturation and lightness values")]
    public class HslSKColorNode : Node
    {
        public HslSKColorNode() : base("HSL Color", "Creates a color from hue, saturation and lightness values")
        {
            H = CreateInputPin<float>("H");
            S = CreateInputPin<float>("S");
            L = CreateInputPin<float>("L");
            Output = CreateOutputPin<SKColor>();
        }

        public InputPin<float> H { get; set; }
        public InputPin<float> S { get; set; }
        public InputPin<float> L { get; set; }
        public OutputPin<SKColor> Output { get; }

        #region Overrides of Node

        /// <inheritdoc />
        public override void Evaluate()
        {
            Output.Value = SKColor.FromHsl(H.Value, S.Value, L.Value);
        }

        #endregion
    }
}