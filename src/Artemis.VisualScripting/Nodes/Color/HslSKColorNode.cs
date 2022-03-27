using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("HSL Color", "Creates a color from hue, saturation and lightness values", "Color", InputType = typeof(Numeric), OutputType = typeof(SKColor))]
public class HslSKColorNode : Node
{
    public HslSKColorNode() : base("HSL Color", "Creates a color from hue, saturation and lightness values")
    {
        H = CreateInputPin<Numeric>("H");
        S = CreateInputPin<Numeric>("S");
        L = CreateInputPin<Numeric>("L");
        Output = CreateOutputPin<SKColor>();
    }

    public InputPin<Numeric> H { get; set; }
    public InputPin<Numeric> S { get; set; }
    public InputPin<Numeric> L { get; set; }
    public OutputPin<SKColor> Output { get; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Output.Value = SKColor.FromHsl(H.Value, S.Value, L.Value);
    }

    #endregion
}