using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.Modules;
using Humanizer;

namespace Artemis.Core;

/// <summary>
///     Represents a collection of output pins for a node capable of outputting the properties of an object or value type.
/// </summary>
public class ObjectOutputPins
{
    private readonly Dictionary<Func<object, object>, OutputPin> _propertyPins;
    private OutputPin? _valueTypePin;

    /// <summary>
    ///     Creates an instance of the <see cref="ObjectOutputPins" /> class.
    /// </summary>
    /// <param name="node">The node the object output was created for.</param>
    public ObjectOutputPins(Node node)
    {
        Node = node;
        _propertyPins = new Dictionary<Func<object, object>, OutputPin>();
    }

    /// <summary>
    ///     Gets the node the object output was created for.
    /// </summary>
    public Node Node { get; }

    /// <summary>
    ///     Gets the current type the node's pins are set up for.
    /// </summary>
    public Type? CurrentType { get; private set; }

    /// <summary>
    ///     Gets a read only collection of the pins outputting the object of this object node.
    /// </summary>
    public ReadOnlyCollection<OutputPin> Pins => _valueTypePin != null ? new ReadOnlyCollection<OutputPin>(new List<OutputPin> {_valueTypePin}) : _propertyPins.Values.ToList().AsReadOnly();

    /// <summary>
    ///     Change the current type and create pins on the node to reflect this.
    /// </summary>
    /// <param name="type">The type to change the collection to.</param>
    public void ChangeType(Type? type)
    {
        if (type == CurrentType)
            return;
        CurrentType = type;

        // Remove current pins
        foreach ((Func<object, object>? _, OutputPin? pin) in _propertyPins)
            Node.RemovePin(pin);
        _propertyPins.Clear();
        if (_valueTypePin != null)
        {
            Node.RemovePin(_valueTypePin);
            _valueTypePin = null;
        }

        if (type == null)
            return;

        // Create new pins
        List<TypeColorRegistration> nodeTypeColors = NodeTypeStore.GetColors();
        if (type.IsClass && type != typeof(string))
            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                Type propertyType = propertyInfo.PropertyType;
                bool toNumeric = Numeric.IsTypeCompatible(propertyType);

                // Skip ignored properties
                if (propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(DataModelIgnoreAttribute)))
                    continue;
                // Skip incompatible properties
                if (!toNumeric && !nodeTypeColors.Any(c => c.Type.IsAssignableFrom(propertyType)))
                    continue;

                // Expect an object
                ParameterExpression itemParameter = Expression.Parameter(typeof(object), "item");
                // Cast it to the actual item type
                UnaryExpression itemCast = Expression.Convert(itemParameter, propertyInfo.DeclaringType!);
                // Access the property
                MemberExpression accessor = Expression.Property(itemCast, propertyInfo);

                // Turn into a numeric if needed or access directly
                UnaryExpression objectExpression;
                if (toNumeric)
                {
                    propertyType = typeof(Numeric);
                    ConstructorInfo constructor = typeof(Numeric).GetConstructors().First(c => c.GetParameters().First().ParameterType == propertyInfo.PropertyType);
                    // Cast the property to an object (sadly boxing)
                    objectExpression = Expression.Convert(Expression.New(constructor, accessor), typeof(object));
                }
                else
                {
                    // Cast the property to an object (sadly boxing)
                    objectExpression = Expression.Convert(accessor, typeof(object));
                }

                // Compile the resulting expression
                Func<object, object> expression = Expression.Lambda<Func<object, object>>(objectExpression, itemParameter).Compile();
                _propertyPins.Add(expression, Node.CreateOrAddOutputPin(propertyType, propertyInfo.Name.Humanize()));
            }
        else
            // Value types are applied directly to a single pin, however if the type is compatible with Numeric, we use a Numeric pin instead
            // the value will then be turned into a numeric in SetCurrentValue
            _valueTypePin = Node.CreateOrAddOutputPin(Numeric.IsTypeCompatible(type) ? typeof(Numeric) : type, "Item");
    }

    /// <summary>
    ///     Set the current value to be output onto connected pins.
    /// </summary>
    /// <param name="value">The value to output onto the connected pins.</param>
    /// <exception cref="ArtemisCoreException"></exception>
    public void SetCurrentValue(object? value)
    {
        if (CurrentType == null)
            throw new ArtemisCoreException("Cannot apply a value to an object output pins not yet configured for a type.");
        if (value != null && CurrentType != value.GetType())
            throw new ArtemisCoreException($"Cannot apply a value of type {value.GetType().FullName} to an object output pins configured for type {CurrentType.FullName}");

        // Apply the object to the pin, it must be connected if SetCurrentValue got called
        if (_valueTypePin != null)
        {
            value ??= _valueTypePin.Type.GetDefault();
            _valueTypePin.Value = _valueTypePin.Type == typeof(Numeric) ? new Numeric(value) : value;
            return;
        }

        // Apply the properties of the object to each connected pin
        foreach ((Func<object, object>? propertyAccessor, OutputPin? outputPin) in _propertyPins)
        {
            if (outputPin.ConnectedTo.Any())
                outputPin.Value = value != null ? propertyAccessor(value) : outputPin.Type.GetDefault();
        }
    }
}