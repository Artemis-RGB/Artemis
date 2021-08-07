using System.Collections;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Greater than", "Checks if the first input is greater than the second.")]
    public class GreaterThanNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input1 { get; }
        public InputPin<object> Input2 { get; }

        public OutputPin<bool> Result { get; }

        #endregion

        #region Constructors

        public GreaterThanNode()
            : base("Greater than", "Checks if the first input is greater than the second.")
        {
            Input1 = CreateInputPin<object>();
            Input2 = CreateInputPin<object>();
            Result = CreateOutputPin<bool>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            try
            {
                Result.Value = Comparer.DefaultInvariant.Compare(Input1.Value, Input2.Value) == 1;
            }
            catch
            {
                Result.Value = false;
            }
        }

        #endregion
    }

    [Node("Less than", "Checks if the first input is less than the second.")]
    public class LessThanNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input1 { get; }
        public InputPin<object> Input2 { get; }

        public OutputPin<bool> Result { get; }

        #endregion

        #region Constructors

        public LessThanNode()
            : base("Less than", "Checks if the first input is less than the second.")
        {
            Input1 = CreateInputPin<object>();
            Input2 = CreateInputPin<object>();
            Result = CreateOutputPin<bool>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            try
            {
                Result.Value = Comparer.DefaultInvariant.Compare(Input1.Value, Input2.Value) == -1;
            }
            catch
            {
                Result.Value = false;
            }
        }

        #endregion
    }

    [Node("Equals", "Checks if the two inputs are equals.")]
    public class EqualsNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input1 { get; }
        public InputPin<object> Input2 { get; }

        public OutputPin<bool> Result { get; }

        #endregion

        #region Constructors

        public EqualsNode()
            : base("Equals", "Checks if the two inputs are equals.")
        {
            Input1 = CreateInputPin<object>();
            Input2 = CreateInputPin<object>();
            Result = CreateOutputPin<bool>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            try
            {
                Result.Value = Equals(Input1.Value, Input2.Value);
            }
            catch
            {
                Result.Value = false;
            }
        }

        #endregion
    }

    [Node("Negate", "Negates the boolean.")]
    public class NegateNode : Node
    {
        #region Properties & Fields

        public InputPin<bool> Input { get; }
        public OutputPin<bool> Output { get; }

        #endregion

        #region Constructors

        public NegateNode()
            : base("Negate", "Negates the boolean.")
        {
            Input = CreateInputPin<bool>();
            Output = CreateOutputPin<bool>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = !Input.Value;
        }

        #endregion
    }
}
