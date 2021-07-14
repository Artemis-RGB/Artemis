using Artemis.VisualScripting.Attributes;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Nodes
{
    [UI("To String", "Converts the input to a string.")]
    public class ConvertToStringNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input { get; }

        public OutputPin<string> String { get; }

        #endregion

        #region Constructors

        public ConvertToStringNode()
            : base("To String", "Converts the input to a string.")
        {
            Input = CreateInputPin<object>();
            String = CreateOutputPin<string>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            String.Value = Input.Value?.ToString();
        }

        #endregion
    }

    [UI("To Integer", "Converts the input to an integer.")]
    public class ConvertToIntegerNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input { get; }

        public OutputPin<int> Integer { get; }

        #endregion

        #region Constructors

        public ConvertToIntegerNode()
            : base("To Integer", "Converts the input to an integer.")
        {
            Input = CreateInputPin<object>();
            Integer = CreateOutputPin<int>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            if (!int.TryParse(Input.Value?.ToString(), out int value))
                value = 0;

            Integer.Value = value;
        }

        #endregion
    }

    [UI("To Double", "Converts the input to a double.")]
    public class ConvertToDoubleNode : Node
    {
        #region Properties & Fields

        public InputPin<object> Input { get; }

        public OutputPin<double> Double { get; }

        #endregion

        #region Constructors

        public ConvertToDoubleNode()
            : base("To Double", "Converts the input to a double.")
        {
            Input = CreateInputPin<object>();
            Double = CreateOutputPin<double>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            if (!double.TryParse(Input.Value?.ToString(), out double value))
                value = 0;

            Double.Value = value;
        }

        #endregion
    }
}
