using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Text is empty", "Outputs true if the input text is empty, false if it contains any text.", 
    "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringNullOrEmptyNode : Node
{
    public StringNullOrEmptyNode()
        : base("Text is empty", "Outputs true if empty")
    {
        Input1 = CreateInputPin<string>();
        Output1 = CreateOutputPin<bool>();
    }

    public InputPin<string> Input1 { get; }

    public OutputPin<bool> Output1 { get; }

    public override void Evaluate()
    {
        bool isNullOrWhiteSpace = string.IsNullOrWhiteSpace(Input1.Value);
        Output1.Value = isNullOrWhiteSpace;
    }
}