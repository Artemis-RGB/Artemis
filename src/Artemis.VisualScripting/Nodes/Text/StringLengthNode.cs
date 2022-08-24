using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("String Length", "Checks whether the first input is contained in the second input.", 
    "Text", InputType = typeof(string), OutputType = typeof(Numeric))]
public class StringLengthNode : Node
{
    public StringLengthNode()
        : base("String Length", "Returns string length.")
    {
        Input1 = CreateInputPin<string>();
        Result = CreateOutputPin<Numeric>();
    }

    public InputPin<string> Input1 { get; }

    public OutputPin<Numeric> Result { get; }

    public override void Evaluate()
    {
        Result.Value = new Numeric((Input1.Value ?? "").Length);
    }
}