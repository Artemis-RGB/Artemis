using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Nodes.Branching;

[Node("Branch", "Forwards one of two values depending on an input boolean", "Branching", InputType = typeof(object), OutputType = typeof(object))]
public class BooleanBranchNode : Node
{
    public BooleanBranchNode()
    {
        BooleanInput = CreateInputPin<bool>();
        TrueInput = CreateInputPin(typeof(object), "True");
        FalseInput = CreateInputPin(typeof(object), "False");

        Output = CreateOutputPin(typeof(object));

        TrueInput.PinConnected += InputPinConnected;
        FalseInput.PinConnected += InputPinConnected;
        TrueInput.PinDisconnected += InputPinDisconnected;
        FalseInput.PinDisconnected += InputPinDisconnected;
    }

    public InputPin<bool> BooleanInput { get; set; }
    public InputPin TrueInput { get; set; }
    public InputPin FalseInput { get; set; }

    public OutputPin Output { get; set; }

    #region Overrides of Node

    /// <inheritdoc />
    public override void Evaluate()
    {
        Output.Value = BooleanInput.Value ? TrueInput.Value : FalseInput.Value;
    }

    #endregion

    private void InputPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        if (TrueInput.ConnectedTo.Any() && !FalseInput.ConnectedTo.Any())
            ChangeType(TrueInput.ConnectedTo.First().Type);
        if (FalseInput.ConnectedTo.Any() && !TrueInput.ConnectedTo.Any())
            ChangeType(FalseInput.ConnectedTo.First().Type);
    }

    private void InputPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        if (!TrueInput.ConnectedTo.Any() && !FalseInput.ConnectedTo.Any())
            ChangeType(typeof(object));
    }

    private void ChangeType(Type type)
    {
        TrueInput.ChangeType(type);
        FalseInput.ChangeType(type);
        Output.ChangeType(type);
    }
}