using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Nodes.Operators.Screens;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Enum Equals", "Determines the equality between an input and a selected enum value", "Operators", InputType = typeof(Enum), OutputType = typeof(bool))]
public class EnumEqualsNode : Node<int, EnumEqualsNodeCustomViewModel>
{
    public EnumEqualsNode() : base("Enum Equals", "Determines the equality between an input and a selected enum value")
    {
        InputPin = CreateInputPin<Enum>();
        OutputPin = CreateOutputPin<bool>();

        InputPin.PinConnected += InputPinOnPinConnected;
    }

    private void InputPinOnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        Storage = 0;
    }

    public InputPin<Enum> InputPin { get; }
    public OutputPin<bool> OutputPin { get; }

    /// <inheritdoc />
    public override void Evaluate()
    {
        if (InputPin.Value == null)
            OutputPin.Value = false;
        else
            OutputPin.Value = Convert.ToInt32(InputPin.Value) == Storage;
    }
}