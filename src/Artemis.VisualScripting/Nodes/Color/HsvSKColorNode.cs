using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("HSV Color", "Creates a color from hue, saturation and value numbers", "Color", InputType = typeof(Numeric), OutputType = typeof(SKColor))]
public class HsvSKColorNode : Node
{
    public HsvSKColorNode()
    {
        H = CreateInputPin<Numeric>("H");
        S = CreateInputPin<Numeric>("S");
        V = CreateInputPin<Numeric>("V");
        Output = CreateOutputPin<SKColor>();
    }

    public InputPin<Numeric> H { get; set; }
    public InputPin<Numeric> S { get; set; }
    public InputPin<Numeric> V { get; set; }
    public OutputPin<SKColor> Output { get; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Output.Value = SKColor.FromHsv(H.Value, S.Value, V.Value);
    }

    #endregion
}