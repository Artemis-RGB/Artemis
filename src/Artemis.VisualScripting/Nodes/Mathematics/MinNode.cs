using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Min", "Outputs the smallest of the connected numeric values.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class MinNumericsNode : Node
{
    #region Constructors

    public MinNumericsNode()
    {
        Values = CreateInputPinCollection<Numeric>("Values", 2);
        Min = CreateOutputPin<Numeric>("Min");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Min.Value = Values.Values.Min();
    }

    #endregion

    #region Properties & Fields

    public InputPinCollection<Numeric> Values { get; }

    public OutputPin<Numeric> Min { get; }

    #endregion
}