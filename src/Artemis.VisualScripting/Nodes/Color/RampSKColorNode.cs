using Artemis.Core;
using Artemis.VisualScripting.Nodes.Color.Screens;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color Ramp", "Maps values to colors with the use of a gradient.", "Color", InputType = typeof(Numeric), OutputType = typeof(SKColor))]
public class RampSKColorNode : Node<ColorGradient, RampSKColorNodeCustomViewModel>
{
    #region Constructors

    public RampSKColorNode()
    {
        Input = CreateInputPin<Numeric>();
        Output = CreateOutputPin<SKColor>();
        Storage = ColorGradient.GetUnicornBarf();
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Storage?.GetColor(Input.Value % 1.0) ?? SKColor.Empty;
    }

    #endregion

    #region Properties & Fields

    public InputPin<Numeric> Input { get; }
    public OutputPin<SKColor> Output { get; }

    #endregion
}