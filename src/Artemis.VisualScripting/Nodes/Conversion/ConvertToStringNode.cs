using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Conversion;

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