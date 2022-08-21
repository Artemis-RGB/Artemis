using System.Diagnostics;
using Artemis.Core;
using Artemis.Core.Events;
using RGB.NET.Core;

namespace Artemis.VisualScripting.Nodes.Timing;

[Node("Delay", "Delays the resolution of the input pin(s) for the given time after each update", "Timing", InputType = typeof(object), OutputType = typeof(object))]
public class DelayNode : Node
{
    #region Properties & Fields

    private long _lastUpdateTimestamp = 0;

    public InputPin<Numeric> Delay { get; }
    public InputPinCollection Input { get; }

    public OutputPin<bool> IsUpdated { get; }
    public OutputPin<Numeric> NextUpdateTime { get; }

    private Dictionary<IPin, OutputPin> _pinPairs = new();

    #endregion

    #region Constructors

    public DelayNode()
        : base("Delay", "Delays the resolution of the input pin(s) for the given time after each update")
    {
        Delay = CreateInputPin<Numeric>("Delay");
        Input = CreateInputPinCollection(typeof(object), initialCount: 0);

        IsUpdated = CreateOutputPin<bool>("Updated");
        NextUpdateTime = CreateOutputPin<Numeric>("Next Update");

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
        double nextUpdateIn = Delay.Value - TimerHelper.GetElapsedTime(_lastUpdateTimestamp);
        NextUpdateTime.Value = nextUpdateIn;

        if (nextUpdateIn <= 0)
        {
            IsUpdated.Value = true;
            foreach ((IPin input, OutputPin output) in _pinPairs)
                output.Value = input.PinValue;

            _lastUpdateTimestamp = Stopwatch.GetTimestamp();
        }
        else
            IsUpdated.Value = false;
    }

    #endregion
}