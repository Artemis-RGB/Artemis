namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class EntryInstallResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Result { get; set; }

    public static EntryInstallResult FromFailure(string? message)
    {
        return new EntryInstallResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static EntryInstallResult FromSuccess(object installationResult)
    {
        return new EntryInstallResult
        {
            IsSuccess = true,
            Result = installationResult
        };
    }
}