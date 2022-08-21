using System;

namespace Artemis.Core;

/// <summary>
///     Represents a data binding registration
/// </summary>
public interface IDataBindingProperty
{
    /// <summary>
    ///     Gets or sets the display name of the data binding registration
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///     Gets the type of the value this data binding registration points to
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    ///     Gets the value of the property this registration points to
    /// </summary>
    /// <returns>A value matching the type of <see cref="ValueType" /></returns>
    object? GetValue();

    /// <summary>
    ///     Sets the value of the property this registration points to
    /// </summary>
    /// <param name="value">A value matching the type of <see cref="ValueType" /></param>
    void SetValue(object? value);
}