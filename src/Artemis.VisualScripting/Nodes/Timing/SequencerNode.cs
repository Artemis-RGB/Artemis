using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Nodes.Timing;

[Node("Sequencer", "Advances on input every time the control has a rising edge (change to true)", "Timing", OutputType = typeof(object))]
public class SequencerNode : Node
{
    #region Properties & Fields

    private int _currentIndex;
    private Type _currentType;
    private bool _updating;
    private IPin? _currentCyclePin;

    private bool _lastInput;

    public InputPin<bool> Input { get; }
    public InputPinCollection CycleValues { get; }

    public OutputPin Output { get; }

    #endregion

    #region Constructors

    public SequencerNode()
        : base("Sequencer", "Advances on input every time the control has a rising edge (change to true)")
    {
        _currentType = typeof(object);

        Input = CreateInputPin<bool>("Control");
        CycleValues = CreateInputPinCollection(typeof(object), "", 0);
        Output = CreateOutputPin(typeof(object));

        CycleValues.PinAdded += CycleValuesOnPinAdded;
        CycleValues.PinRemoved += CycleValuesOnPinRemoved;
        CycleValues.Add(CycleValues.CreatePin());
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        bool input = Input.Value;

        if (input != _lastInput)
        {
            _currentIndex++;

            if (_currentIndex >= CycleValues.Count())
                _currentIndex = 0;

            _currentCyclePin = null;
        }

        _currentCyclePin ??= CycleValues.ElementAt(_currentIndex);

        object? outputValue = _currentCyclePin.PinValue;
        if (Output.Type.IsInstanceOfType(outputValue))
            Output.Value = outputValue;
        else if (Output.Type.IsValueType)
            Output.Value = Output.Type.GetDefault()!;

        _lastInput = input;
    }

    private void CycleValuesOnPinAdded(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected += OnPinConnected;
        e.Value.PinDisconnected += OnPinDisconnected;

        _currentCyclePin = null;
    }

    private void CycleValuesOnPinRemoved(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected -= OnPinConnected;
        e.Value.PinDisconnected -= OnPinDisconnected;

        _currentCyclePin = null;
    }

    private void OnPinDisconnected(object? sender, SingleValueEventArgs<IPin> e) => ProcessPinDisconnected();

    private void OnPinConnected(object? sender, SingleValueEventArgs<IPin> e) => ProcessPinConnected(e.Value);

    private void ProcessPinConnected(IPin source)
    {
        if (_updating)
            return;

        try
        {
            _updating = true;

            // No need to change anything if the types haven't changed
            if (_currentType != source.Type)
                ChangeCurrentType(source.Type);
        }
        finally
        {
            _updating = false;
        }
    }

    private void ChangeCurrentType(Type type)
    {
        CycleValues.ChangeType(type);
        Output.ChangeType(type);

        _currentType = type;
    }

    private void ProcessPinDisconnected()
    {
        if (_updating)
            return;
        try
        {
            // If there's still a connected pin, stick to the current type
            if (CycleValues.Any(v => v.ConnectedTo.Any()))
                return;

            ChangeCurrentType(typeof(object));
        }
        finally
        {
            _updating = false;
        }
    }

    #endregion
}