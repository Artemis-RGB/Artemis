using System;

namespace Artemis.Core;

/// <summary>
///     Provides data for the <see langword='DataBindingPropertyUpdatedEvent' /> event.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataBindingPropertyUpdatedEvent<T> : EventArgs
{
    internal DataBindingPropertyUpdatedEvent(T value)
    {
        Value = value;
    }

    /// <summary>
    ///     The updated value that should be applied to the layer property
    /// </summary>
    public T Value { get; }
}