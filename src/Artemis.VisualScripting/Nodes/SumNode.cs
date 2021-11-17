using Artemis.Core;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Sum", "Sums the connected numeric values.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
    public class SumNumericsNode : Node
    {
        #region Constructors

        public SumNumericsNode()
            : base("Sum", "Sums the connected numeric values.")
        {
            Values = CreateInputPinCollection<Numeric>("Values", 2);
            Sum = CreateOutputPin<Numeric>("Sum");
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Sum.Value = Values.Values.Sum();
        }

        #endregion

        #region Properties & Fields

        public InputPinCollection<Numeric> Values { get; }

        public OutputPin<Numeric> Sum { get; }

        #endregion
    }
}