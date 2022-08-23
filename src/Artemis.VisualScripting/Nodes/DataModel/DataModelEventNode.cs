using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.Events;
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

    private void CreatePins(IDataModelEvent? dataModelEvent)
    {
        if (_dataModelEvent == dataModelEvent)
            return;

        List<IPin> pins = Pins.Skip(2).ToList();
        while (pins.Any())
            RemovePin((Pin) pins.First());
        _propertyPins.Clear();

        _dataModelEvent = dataModelEvent;
        if (dataModelEvent == null)
            return;

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

    public override void Evaluate()
    {
        object? pathValue = _dataModelPath?.GetValue();
        if (pathValue is not IDataModelEvent dataModelEvent)
            return;

        TimeSinceLastTrigger.Value = new Numeric(dataModelEvent.TimeSinceLastTrigger.TotalMilliseconds);
        TriggerCount.Value = new Numeric(dataModelEvent.TriggerCount);
        
        foreach ((Func<DataModelEventArgs, object> propertyAccessor, OutputPin outputPin) in _propertyPins)
        {
            if (!outputPin.ConnectedTo.Any())
                continue;
            object value = dataModelEvent.LastEventArgumentsUntyped != null ? propertyAccessor(dataModelEvent.LastEventArgumentsUntyped) : outputPin.Type.GetDefault()!;
            outputPin.Value = outputPin.IsNumeric ? new Numeric(value) : value;
        }
    }

    private void UpdateDataModelPath()
    {
        DataModelPath? old = _dataModelPath;
        _dataModelPath = Storage != null ? new DataModelPath(Storage) : null;
        old?.Dispose();

        object? pathValue = _dataModelPath?.GetValue();
        if (pathValue is IDataModelEvent dataModelEvent)
            CreatePins(dataModelEvent);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _dataModelPath?.Dispose();
    }
}