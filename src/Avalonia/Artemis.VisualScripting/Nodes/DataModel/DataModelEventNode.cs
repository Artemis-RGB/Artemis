using System.ComponentModel;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.DataModel;

[Node("Data Model-Event", "Responds to a data model event trigger", "Data Model", OutputType = typeof(bool))]
public class DataModelEventNode : Node<DataModelPathEntity, DataModelEventNodeCustomViewModel>, IDisposable
{
    private int _currentIndex;
    private Type _currentType;
    private DataModelPath? _dataModelPath;
    private DateTime _lastTrigger;
    private bool _updating;

    public DataModelEventNode() : base("Data Model-Event", "Responds to a data model event trigger")
    {
        _currentType = typeof(object);

        CycleValues = CreateInputPinCollection(typeof(object), "", 0);
        Output = CreateOutputPin(typeof(object));

        CycleValues.PinAdded += CycleValuesOnPinAdded;
        CycleValues.PinRemoved += CycleValuesOnPinRemoved;
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

        if (Storage == null)
            return;

        UpdateDataModelPath();
    }

    public override void Evaluate()
    {
        object? outputValue = null;
        if (_dataModelPath?.GetValue() is IDataModelEvent dataModelEvent)
        {
            if (dataModelEvent.LastTrigger > _lastTrigger)
            {
                _lastTrigger = dataModelEvent.LastTrigger;
                _currentIndex++;

                if (_currentIndex >= CycleValues.Count())
                    _currentIndex = 0;
            }

            outputValue = CycleValues.ElementAt(_currentIndex).PinValue;
        }

        if (Output.Type.IsInstanceOfType(outputValue))
            Output.Value = outputValue;
        else if (Output.Type.IsValueType)
            Output.Value = Output.Type.GetDefault()!;
    }

    private void CycleValuesOnPinAdded(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected += OnPinConnected;
        e.Value.PinDisconnected += OnPinDisconnected;
    }

    private void CycleValuesOnPinRemoved(object? sender, SingleValueEventArgs<IPin> e)
    {
        e.Value.PinConnected -= OnPinConnected;
        e.Value.PinDisconnected -= OnPinDisconnected;
    }

    private void OnPinDisconnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        ProcessPinDisconnected();
    }

    private void OnPinConnected(object? sender, SingleValueEventArgs<IPin> e)
    {
        ProcessPinConnected(e.Value);
    }

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


    private void UpdateDataModelPath()
    {
        DataModelPath? old = _dataModelPath;
        _dataModelPath = Storage != null ? new DataModelPath(Storage) : null;
        old?.Dispose();
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

    /// <inheritdoc />
    public void Dispose()
    {
        _dataModelPath?.Dispose();
    }
}