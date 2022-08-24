using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("String Null or WhiteSpace", "Checks whether the string is null or white space.", 
    "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringNullOrWhiteSpaceNode : Node
{
    public StringNullOrWhiteSpaceNode()
        : base("Null or White Space", "Returns true if null or white space")
    {
        Input1 = CreateInputPin<string>();
        TrueResult = CreateOutputPin<bool>("true (Empty)");
        FalseResult = CreateOutputPin<bool>("false (Not Empty)");
    }

    public InputPin<string> Input1 { get; }

    public OutputPin<bool> TrueResult { get; }

    public OutputPin<bool> FalseResult { get; }

    public override void Evaluate()
    {
        bool isNullOrWhiteSpace = string.IsNullOrWhiteSpace(Input1.Value);
        TrueResult.Value = isNullOrWhiteSpace;
        FalseResult.Value = !isNullOrWhiteSpace;
    }
}