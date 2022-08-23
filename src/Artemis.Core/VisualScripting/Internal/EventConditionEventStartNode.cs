using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.Modules;
using Artemis.Core.VisualScripting.Internal;
using Humanizer;

namespace Artemis.Core.Internal;

internal class EventConditionEventStartNode : DefaultNode, IEventConditionNode
{
    internal static readonly Guid NodeId = new("278735FE-69E9-4A73-A6B8-59E83EE19305");
    private readonly Dictionary<Func<DataModelEventArgs, object>, OutputPin> _propertyPins;
    private IDataModelEvent? _dataModelEvent;

    public EventConditionEventStartNode() : base(NodeId, "Event Arguments", "Contains the event arguments that triggered the evaluation")
    {
        _propertyPins = new Dictionary<Func<DataModelEventArgs, object>, OutputPin>();
    }

    public void CreatePins(IDataModelEvent? dataModelEvent)
    {
        if (_dataModelEvent == dataModelEvent)
            return;

        while (Pins.Any())
            RemovePin((Pin) Pins.First());
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
        if (_dataModelEvent?.LastEventArgumentsUntyped == null)
            return;

        foreach ((Func<DataModelEventArgs, object> propertyAccessor, OutputPin outputPin) in _propertyPins)
        {
            if (!outputPin.ConnectedTo.Any())
                continue;
            object value = _dataModelEvent.LastEventArgumentsUntyped != null ? propertyAccessor(_dataModelEvent.LastEventArgumentsUntyped) : outputPin.Type.GetDefault()!;
            outputPin.Value = outputPin.IsNumeric ? new Numeric(value) : value;
        }
    }
}