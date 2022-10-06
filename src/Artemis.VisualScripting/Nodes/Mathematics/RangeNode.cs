using Artemis.Core;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Range", "Selects the best integer value in the given range by the given percentage", "Static", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class RangeNode : Node
{
    #region Properties & Fields

    public InputPin<Numeric> Min { get; }
    public InputPin<Numeric> Max { get; }
    public InputPin<Numeric> Percentage { get; }

    public OutputPin<Numeric> Result { get; }

    #endregion

    #region Constructors

    public RangeNode()
    {
        Min = CreateInputPin<Numeric>("Min");
        Max = CreateInputPin<Numeric>("Max");
        Percentage = CreateInputPin<Numeric>("Percentage");

        Result = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate()
    {
        int min = Min.Value;
        int max = Max.Value;
        float percentage = ((float)Percentage.Value).Clamp(0f, 1f);

        int range = max - min;

        Result.Value = percentage >= 1.0f ? max : ((int)(percentage * (range + 1)) + min);
    }

    #endregion
}