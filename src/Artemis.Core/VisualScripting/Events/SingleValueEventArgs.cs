using System;

namespace Artemis.Core.Events;

/// <summary>
///     Represents an event argument containing a single value of type <typeparamref name="T" />
/// </summary>
/// <typeparam name="T">The type of value the argument contains</typeparam>
public class SingleValueEventArgs<T> : EventArgs
{
    #region Constructors

    internal SingleValueEventArgs(T value)
    {
        Value = value;
    }

    #endregion

    #region Properties & Fields

    /// <summary>
    ///     Gets the value of the argument
    /// </summary>
    public T Value { get; }

    #endregion
}