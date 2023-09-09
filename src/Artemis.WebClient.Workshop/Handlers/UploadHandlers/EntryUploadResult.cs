using Artemis.WebClient.Workshop.Entities;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class EntryUploadResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public Release? Release { get; set; }

    public static EntryUploadResult FromFailure(string? message)
    {
        return new EntryUploadResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static EntryUploadResult FromSuccess(Release release)
    {
        return new EntryUploadResult
        {
            IsSuccess = true,
            Release = release
        };
    }
}