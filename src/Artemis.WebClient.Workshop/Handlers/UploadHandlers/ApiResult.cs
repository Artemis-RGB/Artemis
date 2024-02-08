namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static ApiResult FromFailure(string? message)
    {
        return new ApiResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static ApiResult FromSuccess()
    {
        return new ApiResult {IsSuccess = true};
    }
}