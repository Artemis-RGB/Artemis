using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Subtract", "Subtracts the connected numeric values.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class SubtractNumericsNode : Node
{
    #region Constructors

    public SubtractNumericsNode()
    {
        Values = CreateInputPinCollection<Numeric>("Values", 2);
        Remainder = CreateOutputPin<Numeric>("Remainder");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Remainder.Value = Values.Values.Subtract();
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<Numeric> Values { get; }

    public OutputPin<Numeric> Remainder { get; }

    #endregion
}