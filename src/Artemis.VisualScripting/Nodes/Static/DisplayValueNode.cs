using Artemis.Core;
using Artemis.VisualScripting.Nodes.Static.Screens;

namespace Artemis.VisualScripting.Nodes.Static;

[Node("Display Value", "Displays an input value for testing purposes.", "Static", InputType = typeof(object))]
public class DisplayValueNode : Node<string, DisplayValueNodeCustomViewModel>
{
    public DisplayValueNode() : base("Display Value", "Displays an input value for testing purposes.")
    {
        Input = CreateInputPin<object>();
    }

    public InputPin<object> Input { get; }

    public override void Evaluate()
    {
    }
}