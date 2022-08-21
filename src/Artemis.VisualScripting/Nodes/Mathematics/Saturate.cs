using Artemis.Core;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Saturate", "Clamps the value to be in between 0 and 1", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class SaturateNode : Node
{
    #region Properties & Fields

    public InputPin<Numeric> Value { get; }

    public OutputPin<Numeric> Result { get; }

    #endregion

    #region Constructors

    public SaturateNode()
        : base("Clamp", "Clamps the value to be in between 0 and 1")
    {
        Value = CreateInputPin<Numeric>();

        Result = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate() => Result.Value = ((float)Value.Value).Clamp(0f, 1f);

    #endregion
}