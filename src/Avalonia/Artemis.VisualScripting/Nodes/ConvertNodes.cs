using Artemis.Core;

namespace Artemis.VisualScripting.Nodes;

[Node("To String", "Converts the input to a string.", "Conversion", InputType = typeof(object), OutputType = typeof(string))]
public class ConvertToStringNode : Node
{
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

    #region Properties & Fields

    public InputPin<object> Input { get; }

    public OutputPin<string> String { get; }

    #endregion
}

[Node("To Numeric", "Converts the input to a numeric.", "Conversion", InputType = typeof(object), OutputType = typeof(Numeric))]
public class ConvertToNumericNode : Node
{
    #region Constructors

    public ConvertToNumericNode()
        : base("To Numeric", "Converts the input to a numeric.")
    {
        Input = CreateInputPin<object>();
        Output = CreateOutputPin<Numeric>();
    }

    #endregion

    #region Properties & Fields

    public InputPin<object> Input { get; }

    public OutputPin<Numeric> Output { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = Input.Value switch
        {
            int input => new Numeric(input),
            double input => new Numeric(input),
            float input => new Numeric(input),
            byte input => new Numeric(input),
            _ => TryParse(Input.Value)
        };
    }

    private Numeric TryParse(object input)
    {
        Numeric.TryParse(input?.ToString(), out Numeric value);
        return value;
    }

    #endregion
}