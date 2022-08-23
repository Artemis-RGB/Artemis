using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Contains", "Checks whether the first input is contained in the second input.", "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringContainsNode : Node
{
    public StringContainsNode()
        : base("Contains", "Checks whether the first input is contained in the second input.")
    {
        Input1 = CreateInputPin<string>();
        Input2 = CreateInputPin<string>();
        Result = CreateOutputPin<bool>();
    }

    public InputPin<string> Input1 { get; }
    public InputPin<string> Input2 { get; }

    public OutputPin<bool> Result { get; }

    public override void Evaluate()
    {
        if (Input1.Value == null && Input2.Value == null)
            Result.Value = false;
        else if (Input1.Value == null && Input2.Value != null)
            Result.Value = false;
        else if (Input1.Value != null && Input2.Value == null)
            Result.Value = true;
        else if (Input1.Value != null && Input2.Value != null)
            Result.Value = Input1.Value.Contains(Input2.Value, StringComparison.InvariantCultureIgnoreCase);
    }
}