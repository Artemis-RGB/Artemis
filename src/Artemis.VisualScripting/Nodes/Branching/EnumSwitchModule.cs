using Artemis.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.VisualScripting.Nodes.Branching
{
    [Node("Enum Switch", "Returns a different input based on a switch value", "Operators", InputType = typeof(Enum), OutputType = typeof(object))]

    public class EnumSwitchModule : Node
    {
        private readonly Dictionary<Enum, InputPin> _inputPins;

        public OutputPin Output { get; }

        public InputPin<Enum> SwitchValue { get; }

        public EnumSwitchModule() : base("Enum Branch", "desc")
        {
            _inputPins = new();

            Output = CreateOutputPin(typeof(object), "Result");
            SwitchValue = CreateInputPin<Enum>("Switch");

            SwitchValue.PinConnected += OnSwitchPinConnected;
            SwitchValue.PinDisconnected += OnSwitchPinDisconnected;
        }

        public override void Evaluate()
        {
            if (_inputPins.TryGetValue(SwitchValue.Value, out var pin) && pin.ConnectedTo.Count != 0)
            {
                Output.Value = pin.Value;
            }
            else
            {
                Output.Value = null;
            }
        }

        private void ChangeType(Type type)
        {
            foreach (var input in _inputPins.Values)
            {
                input.ChangeType(type);
            }
            Output.ChangeType(type);
        }

        private void OnInputPinDisconnected(object? sender, Core.Events.SingleValueEventArgs<IPin> e)
        {
            if (_inputPins.Values.All(i => i.ConnectedTo.Count == 0))
                ChangeType(typeof(object));
        }

        private void OnInputPinConnected(object? sender, Core.Events.SingleValueEventArgs<IPin> e)
        {
            //change the type of our inputs and output
            //depending on the first node the user connects to
            ChangeType(e.Value.Type);
        }

        private void OnSwitchPinConnected(object? sender, Core.Events.SingleValueEventArgs<IPin> e)
        {
            //the user connected an enum to the switch pin.
            //we need to populate the inputs with all enum options.
            //the type of the input pins is variable, the same as the output.
            var switchType = SwitchValue.ConnectedTo.First().Type;
            if (!switchType.IsEnum)
                return;//what

            foreach (var enumValue in Enum.GetValues(switchType).Cast<Enum>())
            {
                var pin = CreateOrAddInputPin(typeof(object), enumValue.ToString().Humanize(LetterCasing.Sentence));
                pin.PinConnected += OnInputPinConnected;
                pin.PinDisconnected += OnInputPinDisconnected;
                _inputPins.Add(enumValue, pin);
            }
        }

        private void OnSwitchPinDisconnected(object? sender, Core.Events.SingleValueEventArgs<IPin> e)
        {
            foreach (var input in _inputPins.Values)
            {
                input.PinConnected -= OnInputPinConnected;
                input.PinDisconnected -= OnInputPinDisconnected;
                RemovePin(input);
            }

            _inputPins.Clear();
        }
    }
}
