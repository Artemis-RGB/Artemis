using Artemis.Web.Workshop.Entities;

namespace Artemis.WebClient.Workshop.DownloadHandlers;

public class EntryInstallResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }

    public static EntryInstallResult<T> FromFailure(string? message)
    {
        return new EntryInstallResult<T>
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static EntryInstallResult<T> FromSuccess(T installationResult)
    {
        return new EntryInstallResult<T>
        {
            IsSuccess = true,
            Result = installationResult
        };
    }
}