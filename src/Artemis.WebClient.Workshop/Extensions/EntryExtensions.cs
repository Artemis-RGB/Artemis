namespace Artemis.WebClient.Workshop.Extensions;

public static class EntryExtensions
{
    public static string GetEntryPath(this IEntryDetails entry)
    {
        return $"workshop/entries/{entry.EntryType.ToString().ToLower()}s/details/{entry.Id}";
    }
}