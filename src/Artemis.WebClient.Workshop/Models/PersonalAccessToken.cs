namespace Artemis.WebClient.Workshop.Models;

public class PersonalAccessToken
{
    public string Key { get; init; }
    public DateTime CreationTime { get; init; }
    public DateTime? Expiration { get; init; }
    public string? Description { get; init; }
}