using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Format", "Formats the input text.", "Text", InputType = typeof(object), OutputType = typeof(string))]
public class StringFormatNode : Node
{
    #region Constructors

    public StringFormatNode()
    {
        Format = CreateInputPin<string>("Format");
        Values = CreateInputPinCollection<object>("Values");
        Output = CreateOutputPin<string>("Result");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        // Convert numerics to floats beforehand to allow string.Format to format them
        object[] values = Values.Values.Select(v => v is Numeric n ? (float) n : v).ToArray();
        Output.Value = string.Format(Format.Value ?? string.Empty, values);
    }

    #endregion

    #region Properties & Fields

    public InputPin<string> Format { get; }
    public InputPinCollection<object> Values { get; }

    public OutputPin<string> Output { get; }

    #endregion
}