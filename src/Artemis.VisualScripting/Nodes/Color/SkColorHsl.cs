using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color to HSL", "Outputs H, S and L values from a color", "Color", InputType = typeof(SKColor), OutputType = typeof(Numeric))]
public class SkColorHsl : Node
{
    
    public SkColorHsl()
    {
        Input = CreateInputPin<SKColor>();
        H = CreateOutputPin<Numeric>("H");
        S = CreateOutputPin<Numeric>("S");
        L = CreateOutputPin<Numeric>("L");
    }
    
    public InputPin<SKColor> Input { get; }
    public OutputPin<Numeric> H { get; }
    public OutputPin<Numeric> S { get; }
    public OutputPin<Numeric> L { get; }
    
    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Input.Value.ToHsl(out float h, out float s, out float l);
        
        H.Value = h;
        S.Value = s;
        L.Value = l;
    }

    #endregion
}