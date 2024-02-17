namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class EntryUninstallResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static EntryUninstallResult FromFailure(string? message)
    {
        return new EntryUninstallResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static EntryUninstallResult FromSuccess(string? message = null)
    {
        return new EntryUninstallResult
        {
            IsSuccess = true,
            Message = message
        };
    }
}