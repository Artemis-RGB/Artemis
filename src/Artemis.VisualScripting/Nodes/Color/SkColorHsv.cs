using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color to HSV", "Outputs H, S and L values from a color", "Color", InputType = typeof(SKColor), OutputType = typeof(Numeric))]
public class SkColorHsv : Node
{
    
    public SkColorHsv()
    {
        Input = CreateInputPin<SKColor>();
        H = CreateOutputPin<Numeric>("H");
        S = CreateOutputPin<Numeric>("S");
        V = CreateOutputPin<Numeric>("V");
    }
    
    public InputPin<SKColor> Input { get; }
    public OutputPin<Numeric> H { get; }
    public OutputPin<Numeric> S { get; }
    public OutputPin<Numeric> V { get; }
    
    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Input.Value.ToHsv(out float h, out float s, out float v);
        
        H.Value = h;
        S.Value = s;
        V.Value = v;
    }

    #endregion
}