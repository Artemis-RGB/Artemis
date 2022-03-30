using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Nodes.Operators.Screens;

namespace Artemis.VisualScripting.Nodes.Operators;

[Node("Enum Equals", "Determines the equality between an input and a selected enum value", "Operators", InputType = typeof(Enum), OutputType = typeof(bool))]
public class EnumEqualsNode : Node<Enum, EnumEqualsNodeCustomViewModel>
{
    public EnumEqualsNode() : base("Enum Equals", "Determines the equality between an input and a selected enum value")
    {
        InputPin = CreateInputPin<Enum>();
        OutputPin = CreateOutputPin<bool>();

        InputPin.PinConnected += InputPinOnPinConnected;
    }

    private void InputPinOnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        if (Storage?.GetType() != InputPin.ConnectedTo.First().Type)
            Storage = Enum.GetValues(InputPin.ConnectedTo.First().Type).Cast<Enum>().FirstOrDefault();
    }

    public InputPin<Enum> InputPin { get; }
    public OutputPin<bool> OutputPin { get; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        OutputPin.Value = InputPin.Value != null && InputPin.Value.Equals(Storage);
    }

    #endregion
}