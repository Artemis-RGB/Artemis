using System;

namespace Artemis.Core;

/// <inheritdoc />
public class DataBindingProperty<TProperty> : IDataBindingProperty
{
    internal DataBindingProperty(Func<TProperty> getter, Action<TProperty?> setter, string displayName)
    {
        Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        Setter = setter ?? throw new ArgumentNullException(nameof(setter));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }

    /// <summary>
    ///     Gets the function to call to get the value of the property
    /// </summary>
    public Func<TProperty> Getter { get; }

    /// <summary>
    ///     Gets the action to call to set the value of the property
    /// </summary>
    public Action<TProperty?> Setter { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(TProperty);

    /// <inheritdoc />
    public object? GetValue()
    {
        return Getter();
    }

    /// <inheritdoc />
    public void SetValue(object value)
    {
        // Numeric has a bunch of conversion, this seems the cheapest way to use them :)
        switch (value)
        {
            case TProperty match:
                Setter(match);
                break;
            case Numeric numeric when Setter is Action<float> floatSetter:
                floatSetter(numeric);
                break;
            case Numeric numeric when Setter is Action<int> intSetter:
                intSetter(numeric);
                break;
            case Numeric numeric when Setter is Action<double> doubleSetter:
                doubleSetter(numeric);
                break;
            case Numeric numeric when Setter is Action<byte> byteSetter:
                byteSetter(numeric);
                break;
            default:
                throw new ArgumentException("Value must match the type of the data binding registration", nameof(value));
        }
    }
}