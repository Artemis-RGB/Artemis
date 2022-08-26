using Artemis.Core;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Clamp", "Clamps the value to be in between min and max", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class ClampNode : Node
{
    #region Properties & Fields

    public InputPin<Numeric> Value { get; }
    public InputPin<Numeric> Min { get; }
    public InputPin<Numeric> Max { get; }

    public OutputPin<Numeric> Result { get; }

    #endregion

    #region Constructors

    public ClampNode()
        : base("Clamp", "Clamps the value to be in between min and max")
    {
        Value = CreateInputPin<Numeric>("Value");
        Min = CreateInputPin<Numeric>("Min");
        Max = CreateInputPin<Numeric>("Max");

        Result = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate() => Result.Value = ((float)Value.Value).Clamp(Min.Value, Max.Value);

    #endregion
}