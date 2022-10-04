using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Text Length", "Outputs the length of the input text.", 
    "Text", InputType = typeof(string), OutputType = typeof(Numeric))]
public class StringLengthNode : Node
{
    public StringLengthNode()
    {
        Input1 = CreateInputPin<string>();
        Result = CreateOutputPin<Numeric>();
    }

    public InputPin<string> Input1 { get; }

    public OutputPin<Numeric> Result { get; }

    public override void Evaluate()
    {
        Result.Value = Input1.Value == null ? new Numeric(0) : new Numeric(Input1.Value.Length);
    }
}