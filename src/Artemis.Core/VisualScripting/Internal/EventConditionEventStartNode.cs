using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.Modules;
using Artemis.Core.VisualScripting.Internal;
using Humanizer;

namespace Artemis.Core.Internal;

internal class EventConditionEventStartNode : DefaultNode, IEventConditionNode
{
    internal static readonly Guid NodeId = new("278735FE-69E9-4A73-A6B8-59E83EE19305");
    private readonly List<OutputPin> _pinBucket = new();
    private readonly Dictionary<PropertyInfo, OutputPin> _propertyPins;
    private IDataModelEvent? _dataModelEvent;

    public EventConditionEventStartNode() : base(NodeId, "Event Arguments", "Contains the event arguments that triggered the evaluation")
    {
        _propertyPins = new Dictionary<PropertyInfo, OutputPin>();
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

        foreach (PropertyInfo propertyInfo in dataModelEvent.ArgumentsType
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof(DataModelIgnoreAttribute))))
            _propertyPins.Add(propertyInfo, CreateOrAddOutputPin(propertyInfo.PropertyType, propertyInfo.Name.Humanize()));
    }

    /// <summary>
    ///     Creates or adds an input pin to the node using a bucket.
    ///     The bucket might grow a bit over time as the user edits the node but pins won't get lost, enabling undo/redo in the
    ///     editor.
    /// </summary>
    private OutputPin CreateOrAddOutputPin(Type valueType, string displayName)
    {
        // Grab the first pin from the bucket that isn't on the node yet
        OutputPin? pin = _pinBucket.FirstOrDefault(p => !Pins.Contains(p));

        if (Numeric.IsTypeCompatible(valueType))
            valueType = typeof(Numeric);

        // If there is none, create a new one and add it to the bucket
        if (pin == null)
        {
            pin = CreateOutputPin(valueType, displayName);
            _pinBucket.Add(pin);
        }
        // If there was a pin in the bucket, update it's type and display name and reuse it
        else
        {
            pin.ChangeType(valueType);
            pin.Name = displayName;
            AddPin(pin);
        }

        return pin;
    }

    public override void Evaluate()
    {
        if (_dataModelEvent?.LastEventArgumentsUntyped == null)
            return;

        foreach ((PropertyInfo propertyInfo, OutputPin outputPin) in _propertyPins)
        {
            if (outputPin.ConnectedTo.Any())
            {
                object value = propertyInfo.GetValue(_dataModelEvent.LastEventArgumentsUntyped) ?? outputPin.Type.GetDefault()!;
                outputPin.Value = outputPin.IsNumeric ? new Numeric(value) : value;
            }
        }
    }
}