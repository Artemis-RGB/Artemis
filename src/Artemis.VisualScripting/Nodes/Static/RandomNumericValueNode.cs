using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Random", "Generates a random value between 0 and 1", "Static", OutputType = typeof(Numeric))]
public class RandomNumericValueNode : Node
{
    #region Properties & Fields

    private static readonly Random RANDOM = new();

    public OutputPin<Numeric> Output { get; }

    #endregion

    #region Constructors

    public RandomNumericValueNode()
    {
        Output = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void Evaluate() => Output.Value = RANDOM.NextSingle();

    #endregion
}