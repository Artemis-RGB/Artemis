namespace Artemis.WebClient.Workshop.Exceptions;

/// <summary>
///     An exception thrown when a workshop related error occurs
/// </summary>
public class ArtemisWorkshopException : Exception
{
    /// <inheritdoc />
    public ArtemisWorkshopException()
    {
    }

    /// <inheritdoc />
    public ArtemisWorkshopException(string? message) : base(message)
    {
    }

    /// <inheritdoc />
    public ArtemisWorkshopException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}