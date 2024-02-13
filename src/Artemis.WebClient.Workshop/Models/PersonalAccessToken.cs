namespace Artemis.WebClient.Workshop.Models;

public record PersonalAccessToken(string Key, DateTime CreationTime, DateTime? Expiration, string? Description);