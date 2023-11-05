namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class ImageUploadRequest
{
    public ImageUploadRequest(Stream file, string name, string? description)
    {
        File = file;
        Name = name.Length > 50 ? name.Substring(0, 50) : name;
        if (description != null)
            Description = description.Length > 150 ? description.Substring(0, 150) : description;
    }

    public Stream File { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}