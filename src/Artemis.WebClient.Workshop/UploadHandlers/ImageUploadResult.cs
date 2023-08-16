namespace Artemis.WebClient.Workshop.UploadHandlers;

public class ImageUploadResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static ImageUploadResult FromFailure(string? message)
    {
        return new ImageUploadResult
        {
            IsSuccess = false,
            Message = message
        };
    }

    public static ImageUploadResult FromSuccess()
    {
        return new ImageUploadResult {IsSuccess = true};
    }
}