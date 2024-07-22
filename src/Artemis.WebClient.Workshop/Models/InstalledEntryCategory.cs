namespace Artemis.WebClient.Workshop.Models;

public class InstalledEntryCategory
{
    public InstalledEntryCategory(string name, string icon)
    {
        Name = name;
        Icon = icon;
    }

    public string Name { get; }

    public string Icon { get; }
}