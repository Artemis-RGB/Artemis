using Artemis.Core;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Lerp", "Interpolates linear between the two values A and B", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class LerpNode : Node
{
    #region Properties & Fields

    public InputPin<Numeric> A { get; }
    public InputPin<Numeric> B { get; }
    public InputPin<Numeric> T { get; }

    public OutputPin<Numeric> Result { get; }

    #endregion

    #region Constructors

    public LerpNode()
        : base("Lerp", "Interpolates linear between the two values A and B")
    {
        A = CreateInputPin<Numeric>("A");
        B = CreateInputPin<Numeric>("B");
        T = CreateInputPin<Numeric>("T");

        Result = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate()
    {
        float a = A.Value;
        float b = B.Value;
        float t = ((float)T.Value).Clamp(0f, 1f);
        Result.Value = ((b - a) * t) + a;
    }

    #endregion
}