using Artemis.Core;
using Artemis.Core.Events;
using Humanizer;

namespace Artemis.VisualScripting.Nodes.Branching;

[Node("Switch (Enum)", "Outputs the input that corresponds to the switch value", "Operators", InputType = typeof(Enum), OutputType = typeof(object))]
public class EnumSwitchNode : Node
{
    private readonly Dictionary<Enum, InputPin> _inputPins;

    public EnumSwitchNode() : base("Enum Branch", "desc")
    {
        _inputPins = new Dictionary<Enum, InputPin>();

        Output = CreateOutputPin(typeof(object), "Result");
        SwitchValue = CreateInputPin<Enum>("Switch");

        SwitchValue.PinConnected += OnSwitchPinConnected;
        SwitchValue.PinDisconnected += OnSwitchPinDisconnected;
    }

    public OutputPin Output { get; }
    public InputPin<Enum> SwitchValue { get; }

    public override void Evaluate()
    {
        if (SwitchValue.Value is null)
        {
            Output.Value = null;
            return;
        }

        if (!_inputPins.TryGetValue(SwitchValue.Value, out InputPin? pin))
        {
            Output.Value = null;
            return;
        }

        if (pin.ConnectedTo.Count == 0)
        {
            Output.Value = null;
            return;
        }

        Output.Value = pin.Value;
    }

    private void OnInputPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        // if this is the last pin to disconnect, reset the type.
        if (_inputPins.Values.All(i => i.ConnectedTo.Count == 0))
            ChangeType(typeof(object));
    }

    private void OnInputPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        // change the type of our inputs and output
        // depending on the first node the user connects to
        ChangeType(e.Value.Type);
    }

    private void OnSwitchPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        if (SwitchValue.ConnectedTo.Count == 0)
            return;

        Type enumType = SwitchValue.ConnectedTo[0].Type;
        foreach (Enum enumValue in Enum.GetValues(enumType).Cast<Enum>())
        {
            InputPin pin = CreateOrAddInputPin(typeof(object), enumValue.ToString().Humanize(LetterCasing.Sentence));
            pin.PinConnected += OnInputPinConnected;
            pin.PinDisconnected += OnInputPinDisconnected;
            _inputPins[enumValue] = pin;
        }
    }

    private void OnSwitchPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        foreach (InputPin input in _inputPins.Values)
        {
            input.PinConnected -= OnInputPinConnected;
            input.PinDisconnected -= OnInputPinDisconnected;
            RemovePin(input);
        }

        _inputPins.Clear();
        ChangeType(typeof(object));
    }

    private void ChangeType(Type type)
    {
        foreach (InputPin input in _inputPins.Values)
            input.ChangeType(type);
        Output.ChangeType(type);
    }
}