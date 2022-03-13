using Artemis.Core;

namespace Artemis.VisualScripting.Nodes;

[Node("Format", "Formats the input string.", "Text", InputType = typeof(object), OutputType = typeof(string))]
public class StringFormatNode : Node
{
    #region Constructors

    public StringFormatNode()
        : base("Format", "Formats the input string.")
    {
        Format = CreateInputPin<string>("Format");
        Values = CreateInputPinCollection<object>("Values");
        Output = CreateOutputPin<string>("Result");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        Output.Value = string.Format(Format.Value ?? string.Empty, Values.Values.ToArray());
    }

    #endregion

    #region Properties & Fields

    public InputPin<string> Format { get; }
    public InputPinCollection<object> Values { get; }

    public OutputPin<string> Output { get; }

    #endregion
}