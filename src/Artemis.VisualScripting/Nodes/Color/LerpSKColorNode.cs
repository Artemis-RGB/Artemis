using Artemis.Core;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Lerp (Color)", "Interpolates linear between the two colors A and B", "Color", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
public class LerpSKColorNode : Node
{
    #region Properties & Fields

    public InputPin<SKColor> A { get; }
    public InputPin<SKColor> B { get; }
    public InputPin<Numeric> T { get; }

    public OutputPin<SKColor> Result { get; }

    #endregion

    #region Constructors

    public LerpSKColorNode()
    {
        Name = "Lerp";
        A = CreateInputPin<SKColor>("A");
        B = CreateInputPin<SKColor>("B");
        T = CreateInputPin<Numeric>("T");

        Result = CreateOutputPin<SKColor>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate()
    {
        SKColor a = A.Value;
        SKColor b = B.Value;
        float t = ((float)T.Value).Clamp(0f, 1f);

        float aAlpha = a.Alpha.GetPercentageFromByteValue();
        float aRed = a.Red.GetPercentageFromByteValue();
        float aGreen = a.Green.GetPercentageFromByteValue();
        float aBlue = a.Blue.GetPercentageFromByteValue();

        float alpha = ((b.Alpha.GetPercentageFromByteValue() - aAlpha) * t) + aAlpha;
        float red = ((b.Red.GetPercentageFromByteValue() - aRed) * t) + aRed;
        float green = ((b.Green.GetPercentageFromByteValue() - aGreen) * t) + aGreen;
        float blue = ((b.Blue.GetPercentageFromByteValue() - aBlue) * t) + aBlue;

        Result.Value = new SKColor(red.GetByteValueFromPercentage(), green.GetByteValueFromPercentage(), blue.GetByteValueFromPercentage(), alpha.GetByteValueFromPercentage());
    }

    #endregion
}