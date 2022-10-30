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
        // Wrap the input between 0 and 1
        // 1 % 1 = 0, 2 % 1 = 0 etc. but we want that to be 1 but 0 should stay 0, call me stupid but this works and makes sense
        float value = Input.Value % 1;
        if (value == 0 && Input.Value != 0)
            value = 1;
        
        Output.Value = Storage?.GetColor(value) ?? SKColor.Empty;
    }

    #endregion

    #region Properties & Fields

    public InputPin<Numeric> Input { get; }
    public OutputPin<SKColor> Output { get; }

    #endregion
}