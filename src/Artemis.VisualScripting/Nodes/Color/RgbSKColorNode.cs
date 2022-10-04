using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("RGB Color", "Creates a color from red, green and blue values", "Color", InputType = typeof(Numeric), OutputType = typeof(SKColor))]
public class RgbSKColorNode : Node
{
    #region Properties & Fields

    public InputPin<Numeric> R { get; set; }
    public InputPin<Numeric> G { get; set; }
    public InputPin<Numeric> B { get; set; }
    public OutputPin<SKColor> Output { get; }

    #endregion

    #region Constructors

    public RgbSKColorNode()
    {
        R = CreateInputPin<Numeric>("R");
        G = CreateInputPin<Numeric>("G");
        B = CreateInputPin<Numeric>("B");

        Output = CreateOutputPin<SKColor>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate() => Output.Value = new SKColor(R.Value, G.Value, B.Value);

    #endregion
}