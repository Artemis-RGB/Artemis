using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("String Null or WhiteSpace", "Checks whether the string is null, empty or white space.", 
    "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringNullOrWhiteSpaceNode : Node
{
    public StringNullOrWhiteSpaceNode()
        : base("Null or White Space", "Returns true if null or white space")
    {
        Input1 = CreateInputPin<string>();
        NullOrWhiteSpaceResult = CreateOutputPin<bool>("White Space");
        HasContentResult = CreateOutputPin<bool>("Has Content");
    }

    public InputPin<string> Input1 { get; }

    public OutputPin<bool> NullOrWhiteSpaceResult { get; }

    public OutputPin<bool> HasContentResult { get; }

    public override void Evaluate()
    {
        bool isNullOrWhiteSpace = string.IsNullOrWhiteSpace(Input1.Value);
        NullOrWhiteSpaceResult.Value = isNullOrWhiteSpace;
        HasContentResult.Value = !isNullOrWhiteSpace;
    }
}