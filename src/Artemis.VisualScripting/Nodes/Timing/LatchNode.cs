using System.Diagnostics;
using Artemis.Core;
using Artemis.Core.Events;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Timing;

[Node("Latch", "Only passes the input to the output as long as the control-pin is true. If the control pin is false the last passed value is provided.", "Timing", InputType = typeof(object), OutputType = typeof(object))]
public class LatchNode : Node
{
    #region Properties & Fields

    private long _lastUpdateTimestamp = 0;

    public InputPin<bool> Control { get; }
    public InputPinCollection Input { get; }

    //TODO DarthAffe 21.08.2022:  Find something to output to aling in- and outputs
    public OutputPin<Numeric> LastUpdateTime { get; }

    private Dictionary<IPin, OutputPin> _pinPairs = new();

    #endregion

    #region Constructors

    public LatchNode()
        : base("Latch", "Only passes the input to the output as long as the control-pin is true. If the control pin is false the last passed value is provided.")
    {
        Control = CreateInputPin<bool>("Control");
        Input = CreateInputPinCollection(typeof(object), initialCount: 0);

        LastUpdateTime = CreateOutputPin<Numeric>("Last Update");

        Input.PinAdded += OnInputPinAdded;
        Input.PinRemoved += OnInputPinRemoved;

        Input.Add(Input.CreatePin());
    }

    #endregion

    #region Methods

    private void OnInputPinAdded(object? sender, SingleValueEventArgs<IPin> args)
    {
        IPin inputPin = args.Value;
        _pinPairs.Add(inputPin, CreateOutputPin(typeof(object)));

        inputPin.PinConnected += OnInputPinConnected;
        inputPin.PinDisconnected += OnInputPinDisconnected;

        UpdatePinNames();
    }

    private void OnInputPinRemoved(object? sender, SingleValueEventArgs<IPin> args)
    {
        IPin inputPin = args.Value;
        RemovePin(_pinPairs[inputPin]);
        _pinPairs.Remove(inputPin);

        inputPin.PinConnected -= OnInputPinConnected;
        inputPin.PinDisconnected -= OnInputPinDisconnected;

        UpdatePinNames();
    }

    private void OnInputPinConnected(object? sender, SingleValueEventArgs<IPin> args)
    {
        if (sender is not IPin inputPin || !_pinPairs.ContainsKey(inputPin)) return;

        OutputPin outputPin = _pinPairs[inputPin];
        outputPin.ChangeType(args.Value.Type);
    }

    private void OnInputPinDisconnected(object? sender, SingleValueEventArgs<IPin> args)
    {
        if (sender is not IPin inputPin || !_pinPairs.ContainsKey(inputPin)) return;

        OutputPin outputPin = _pinPairs[inputPin];
        outputPin.ChangeType(typeof(object));
    }

    private void UpdatePinNames()
    {
        int counter = 1;
        foreach (IPin inputPin in Input.Pins)
        {
            string name = counter.ToString();
            inputPin.Name = name;
            _pinPairs[inputPin].Name = name;

            counter++;
        }
    }

    /// <inheritdoc />
    public override void Evaluate()
    {
        if (Control.Value)
        {
            foreach ((IPin input, OutputPin output) in _pinPairs)
                output.Value = input.PinValue;

            LastUpdateTime.Value = 0;
            _lastUpdateTimestamp = Stopwatch.GetTimestamp();
        }
        else
            LastUpdateTime.Value = TimerHelper.GetElapsedTime(_lastUpdateTimestamp);
    }

    #endregion
}