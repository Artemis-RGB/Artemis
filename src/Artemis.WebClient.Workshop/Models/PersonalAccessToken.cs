namespace Artemis.WebClient.Workshop.Models;

public class PersonalAccessToken
{
    public string Key { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public DateTimeOffset? Expiration { get; init; }
    public string? Description { get; init; }
}