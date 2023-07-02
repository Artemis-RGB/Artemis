using System;

namespace Artemis.UI.Shared;

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