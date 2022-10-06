using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Conversion;

[Node("To Text", "Converts the input to text.", "Conversion", InputType = typeof(object), OutputType = typeof(string))]
public class ConvertToStringNode : Node
{
    #region Constructors

    public ConvertToStringNode()
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