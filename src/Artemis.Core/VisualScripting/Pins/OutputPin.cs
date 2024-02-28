using System;

namespace Artemis.Core;

/// <summary>
///     Represents an output pin containing a value of type <typeparamref name="T" /> on a <see cref="INode" />
/// </summary>
public sealed class OutputPin<T> : Pin
{
    #region Constructors

    internal OutputPin(INode node, string name)
        : base(node, name)
    {
        _value = default;
        IsNumeric = typeof(T) == typeof(Numeric);
    }

    #endregion

    #region Properties & Fields

    /// <inheritdoc />
    public override Type Type { get; } = typeof(T);

    /// <inheritdoc />
    public override object? PinValue => Value;

    /// <inheritdoc />
    public override PinDirection Direction => PinDirection.Output;

    private T? _value;

    /// <summary>
    ///     Gets or sets the value of the output pin
    /// </summary>
    public T? Value
    {
        get
        {
            if (!IsEvaluated)
                Node?.TryEvaluate();

            return _value;
        }
        set
        {
            _value = value;
            IsEvaluated = true;

            OnPropertyChanged(nameof(PinValue));
        }
    }

    #endregion
}

/// <summary>
///     Represents an output pin on a <see cref="INode" />
/// </summary>
public sealed class OutputPin : Pin
{
    #region Constructors

    internal OutputPin(INode node, Type type, string name)
        : base(node, name)
    {
        _type = type;
        _value = type.GetDefault();
        IsNumeric = type == typeof(Numeric);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Changes the type of this pin, disconnecting any pins that are incompatible with the new type.
    /// </summary>
    /// <param name="type">The new type of the pin.</param>
    public void ChangeType(Type type)
    {
        if (type == _type)
            return;

        base.ChangeType(type, ref _type);
        Value = type.GetDefault();
    }

    #endregion

    #region Properties & Fields

    /// <inheritdoc />
    public override Type Type => _type;

    /// <inheritdoc />
    public override object? PinValue => Value;

    /// <inheritdoc />
    public override PinDirection Direction => PinDirection.Output;

    private object? _value;
    private Type _type;

    /// <summary>
    ///     Gets or sets the value of the output pin
    /// </summary>
    public object? Value
    {
        get
        {
            if (!IsEvaluated)
                Node?.TryEvaluate();

            return _value;
        }
        set
        {
            if (Type.IsValueType && value == null)
                // We can't take null for value types so set it to the default value for that type
                _value = Type.GetDefault();
            else if (value != null)
                // If a value was given make sure it matches
                if (!Type.IsInstanceOfType(value))
                    throw new ArgumentException($"Value of type '{value.GetType().Name}' can't be assigned to a pin of type {Type.Name}.");

            // Otherwise we're good and we can put a null here if it happens to be that
            _value = value;
            IsEvaluated = true;
            OnPropertyChanged(nameof(PinValue));
        }
    }

    #endregion
}