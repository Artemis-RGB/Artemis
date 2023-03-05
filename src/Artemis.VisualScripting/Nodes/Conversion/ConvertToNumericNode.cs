using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Conversion;

[Node("To Numeric", "Converts the input to a numeric.", "Conversion", InputType = typeof(object), OutputType = typeof(Numeric))]
public class ConvertToNumericNode : Node
{
    #region Constructors

    public ConvertToNumericNode()
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
            bool input => new Numeric(input ? 1 : 0),
            _ => TryParse(Input.Value)
        };
    }

    private Numeric TryParse(object? input)
    {
        Numeric.TryParse(input?.ToString(), out Numeric value);
        return value;
    }

    #endregion
}
