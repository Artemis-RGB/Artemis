using System;

namespace Artemis.UI.Exceptions;

public class ArtemisGraphicsContextException : Exception
{
    /// <inheritdoc />
    public ArtemisGraphicsContextException()
    {
    }

    /// <inheritdoc />
    public ArtemisGraphicsContextException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArtemisGraphicsContextException(string message, Exception innerException) : base(message, innerException)
    {
    }
}