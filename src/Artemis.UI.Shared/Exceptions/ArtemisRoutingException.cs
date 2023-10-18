using System;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents errors that occur within the Artemis router.
/// </summary>
public class ArtemisRoutingException : Exception
{
    /// <inheritdoc />
    public ArtemisRoutingException()
    {
    }

    /// <inheritdoc />
    public ArtemisRoutingException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArtemisRoutingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}