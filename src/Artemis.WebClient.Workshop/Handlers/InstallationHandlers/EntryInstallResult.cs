using Artemis.Core;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class EntryInstallResult
{
    /// <summary>
    /// Gets a value indicating whether the installation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets a message describing the result of the installation.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Gets an exception thrown during the installation process, if any.
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// Gets the entry that was installed, if any.
    /// </summary>
    public InstalledEntry? Entry { get; private set; }

    /// <summary>
    /// Gets the result object returned by the installation handler, if any.
    /// <remarks>This'll be a <see cref="ProfileConfiguration"/>, <see cref="ArtemisLayout"/> or <see cref="Plugin"/> depending on the entry type.</remarks>
    /// </summary>
    public object? Installed { get; private set; }

    public static EntryInstallResult FromFailure(string? message)
    {
        return new EntryInstallResult
        {
            IsSuccess = false,
            Message = message
        };
    }
    
    public static EntryInstallResult FromException(Exception exception)
    {
        return new EntryInstallResult
        {
            IsSuccess = false,
            Message = exception.Message,
            Exception = exception
        };
    }

    public static EntryInstallResult FromSuccess(InstalledEntry installedEntry, object? result)
    {
        return new EntryInstallResult
        {
            IsSuccess = true,
            Entry = installedEntry,
            Installed = result
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(IsSuccess)}: {IsSuccess}, {nameof(Message)}: {Message}";
    }
}