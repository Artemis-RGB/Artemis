using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.VisualScripting.Nodes.DataModel.Screens;
using Humanizer;

namespace Artemis.VisualScripting.Nodes.DataModel;

[Node("Data Model-Event", "Outputs the latest values of a data model event.", "Data Model", OutputType = typeof(object))]
public class DataModelEventNode : Node<DataModelPathEntity, DataModelEventNodeCustomViewModel>, IDisposable
{
    private readonly Dictionary<Func<DataModelEventArgs, object>, OutputPin> _propertyPins;
    private DataModelPath? _dataModelPath;
    private IDataModelEvent? _dataModelEvent;
    private OutputPin? _oldValuePin;
    private OutputPin? _newValuePin;
    private DateTime _lastTrigger;
    private object? _lastValue;
    private int _valueChangeCount;

    public DataModelEventNode() : base("Data Model-Event", "Outputs the latest values of a data model event.")
    {
        _propertyPins = new Dictionary<Func<DataModelEventArgs, object>, OutputPin>();
        
        TimeSinceLastTrigger = CreateOutputPin<Numeric>("Time since trigger");
        TriggerCount = CreateOutputPin<Numeric>("Trigger count");

        // Monitor storage for changes
        StorageModified += (_, _) => UpdateDataModelPath();
    }

    public INodeScript? Script { get; set; }
    public OutputPin<Numeric> TimeSinceLastTrigger { get; }
    public OutputPin<Numeric> TriggerCount { get; }

    public override void Initialize(INodeScript script)
    {
        Script = script;

        if (Storage == null)
            return;

        UpdateDataModelPath();
    }

    public override void Evaluate()
    {
        object? pathValue = _dataModelPath?.GetValue();
        
        // If the path is a data model event, evaluate the event
        if (pathValue is IDataModelEvent dataModelEvent)
        {
            TimeSinceLastTrigger.Value = dataModelEvent.TimeSinceLastTrigger.TotalMilliseconds;
            TriggerCount.Value = dataModelEvent.TriggerCount;

            foreach ((Func<DataModelEventArgs, object> propertyAccessor, OutputPin outputPin) in _propertyPins)
            {
                if (!outputPin.ConnectedTo.Any())
                    continue;
                object value = dataModelEvent.LastEventArgumentsUntyped != null ? propertyAccessor(dataModelEvent.LastEventArgumentsUntyped) : outputPin.Type.GetDefault()!;
                outputPin.Value = outputPin.IsNumeric ? new Numeric(value) : value;
            }
        }
        // If the path is a regular value, evaluate the current value
        else if (_oldValuePin != null && _newValuePin != null)
        {
            if (Equals(_lastValue, pathValue))
            {
                TimeSinceLastTrigger.Value = (DateTime.Now - _lastTrigger).TotalMilliseconds;
                return;
            }
           
            _valueChangeCount++;
            _lastTrigger = DateTime.Now;
            
            _oldValuePin.Value = _lastValue;
            _newValuePin.Value = pathValue;
            
            _lastValue = pathValue;

            TimeSinceLastTrigger.Value = 0;
            TriggerCount.Value = _valueChangeCount;
        }
    }

    private void UpdateDataModelPath()
    {
        DataModelPath? old = _dataModelPath;
        _dataModelPath = Storage != null ? new DataModelPath(Storage) : null;
        if (_dataModelPath != null)
            _dataModelPath.PathValidated += DataModelPathOnPathValidated;
        
        if (old != null)
        {
            old.PathValidated -= DataModelPathOnPathValidated;
            old.Dispose();
        }
        
        UpdateOutputPins();
    }

    private void UpdateOutputPins()
    {
        object? pathValue = _dataModelPath?.GetValue();
        if (pathValue is IDataModelEvent dataModelEvent)
            CreateEventPins(dataModelEvent);
        else
            CreateValuePins();
    }

    private void CreateEventPins(IDataModelEvent dataModelEvent)
    {
        if (_dataModelEvent == dataModelEvent)
            return;
        
        ClearPins();
        _dataModelEvent = dataModelEvent;
        foreach (PropertyInfo propertyInfo in dataModelEvent.ArgumentsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DataModelIgnoreAttribute))))
        {
            // Expect an IDataModelEvent
            ParameterExpression eventParameter = Expression.Parameter(typeof(DataModelEventArgs), "event");
            // Cast it to the actual event type
            UnaryExpression eventCast = Expression.Convert(eventParameter, propertyInfo.DeclaringType!);
            // Access the property
            MemberExpression accessor = Expression.Property(eventCast, propertyInfo);
            // Cast the property to an object (sadly boxing)
            UnaryExpression objectCast = Expression.Convert(accessor, typeof(object));
            // Compile the resulting expression
            Func<DataModelEventArgs, object> expression = Expression.Lambda<Func<DataModelEventArgs, object>>(objectCast, eventParameter).Compile();

            _propertyPins.Add(expression, CreateOrAddOutputPin(propertyInfo.PropertyType, propertyInfo.Name.Humanize()));
        }
    }

    private void CreateValuePins()
    {
        ClearPins();

        Type? propertyType = _dataModelPath?.GetPropertyType();
        if (propertyType == null)
            return;
        
        _oldValuePin = CreateOrAddOutputPin(propertyType, "Old value");
        _newValuePin = CreateOrAddOutputPin(propertyType, "New value");
        _lastValue = null;
        _valueChangeCount = 0;
    }
    
    private void ClearPins()
    {
        List<IPin> pins = Pins.Skip(2).ToList();
        foreach (IPin pin in pins)
            RemovePin((Pin) pin);
        
        _propertyPins.Clear();
        _oldValuePin = null;
        _newValuePin = null;
    }
    
    private void DataModelPathOnPathValidated(object? sender, EventArgs e)
    {
        // Update the output pin now that the type is known and attempt to restore the connection that was likely missing
        UpdateOutputPins();
        Script?.LoadConnections();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _dataModelPath?.Dispose();
    }
}