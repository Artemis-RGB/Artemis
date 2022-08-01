using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Max", "Outputs the largest of the connected numeric values.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class MaxNumericsNode : Node
{
    #region Constructors

    public MaxNumericsNode()
        : base("Max", "Outputs the largest of the connected numeric values.")
    {
        Values = CreateInputPinCollection<Numeric>("Values", 2);
        Max = CreateOutputPin<Numeric>("Max");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Max.Value = Values.Values.Max();
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<Numeric> Values { get; }

    public OutputPin<Numeric> Max { get; }

    #endregion
}