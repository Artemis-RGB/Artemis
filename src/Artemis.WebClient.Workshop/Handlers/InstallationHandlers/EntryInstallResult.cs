using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class EntryInstallResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public InstalledEntry? Entry { get; set; }

    public static EntryInstallResult FromFailure(string? message)
    {
        return new EntryInstallResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static EntryInstallResult FromSuccess(InstalledEntry installedEntry)
    {
        return new EntryInstallResult
        {
            IsSuccess = true,
            Entry = installedEntry
        };
    }
}