namespace Artemis.WebClient.Workshop.Exceptions;

/// <summary>
///     An exception thrown when a web client related error occurs
/// </summary>
public class ArtemisWebClientException : Exception
{
    /// <inheritdoc />
    public ArtemisWebClientException()
    {
    }

    /// <inheritdoc />
    public ArtemisWebClientException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArtemisWebClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}