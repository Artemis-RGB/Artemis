using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.Screens;

namespace Artemis.VisualScripting.Nodes.DataModel;

[Node("Data Model-Event Value Cycle", "Cycles through provided values each time the select event fires.", "Data Model", OutputType = typeof(object))]
public class DataModelEventCycleNode : Node<DataModelPathEntity, DataModelEventCycleNodeCustomViewModel>, IDisposable
{
    private int _currentIndex;
    private Type _currentType;
    private DataModelPath? _dataModelPath;
    private IDataModelEvent? _subscribedEvent;
    private object? _lastPathValue;
    private bool _updating;

    public DataModelEventCycleNode()
    {
        _currentType = typeof(object);

        CycleValues = CreateInputPinCollection(typeof(object), "", 0);
        Output = CreateOutputPin(typeof(object));

        CycleValues.PinAdded += OnCycleValuesOnPinAdded;
        CycleValues.PinRemoved += OnCycleValuesOnPinRemoved;
        CycleValues.Add(CycleValues.CreatePin());

        // Monitor storage for changes
        StorageModified += (_, _) => UpdateDataModelPath();
    }

    public INodeScript? Script { get; set; }

    public InputPinCollection CycleValues { get; }
    public OutputPin Output { get; }

    public override void Initialize(INodeScript script)
    {
        Script = script;

        if (Storage != null)
            UpdateDataModelPath();
    }

    public override void Evaluate()
    {
        object? pathValue = _dataModelPath?.GetValue();
        if (pathValue is not IDataModelEvent && EvaluateValue(pathValue))
            Cycle();

        object? outputValue = CycleValues.ElementAt(_currentIndex).PinValue;
        if (Output.Type.IsInstanceOfType(outputValue))
            Output.Value = outputValue;
        else if (Output.Type.IsValueType)
            Output.Value = Output.Type.GetDefault()!;
    }

    private bool EvaluateValue(object? pathValue)
    {
        if (Equals(pathValue, _lastPathValue))
            return false;

        _lastPathValue = pathValue;
        return true;
    }

    private void Cycle()
    {
        _currentIndex++;

        if (_currentIndex >= CycleValues.Count())
            _currentIndex = 0;
    }

    private void UpdateDataModelPath()
    {
        DataModelPath? old = _dataModelPath;

        if (_subscribedEvent != null)
        {
            _subscribedEvent.EventTriggered -= OnEventTriggered;
            _subscribedEvent = null;
        }

        _dataModelPath = Storage != null ? new DataModelPath(Storage) : null;

        if (_dataModelPath?.GetValue() is IDataModelEvent newEvent)
        {
            _subscribedEvent = newEvent;
            _subscribedEvent.EventTriggered += OnEventTriggered;
        }

        old?.Dispose();
    }

    private void ChangeCurrentType(Type type)
    {
        CycleValues.ChangeType(type);
        Output.ChangeType(type);

        _currentType = type;
    }

    private void OnEventTriggered(object? sender, EventArgs e)
    {
        Cycle();
    }

    private void OnCycleValuesOnPinAdded(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected += OnPinConnected;
        e.Value.PinDisconnected += OnPinDisconnected;
    }

    private void OnCycleValuesOnPinRemoved(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected -= OnPinConnected;
        e.Value.PinDisconnected -= OnPinDisconnected;
    }

    private void OnPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
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

    private void OnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        if (_updating)
            return;

        try
        {
            _updating = true;

            // No need to change anything if the types haven't changed
            if (_currentType != e.Value.Type)
                ChangeCurrentType(e.Value.Type);
        }
        finally
        {
            _updating = false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_subscribedEvent != null)
        {
            _subscribedEvent.EventTriggered -= OnEventTriggered;
            _subscribedEvent = null;
        }

        _dataModelPath?.Dispose();
    }
}