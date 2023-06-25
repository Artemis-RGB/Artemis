using System;

namespace Artemis.UI.Screens.Settings;

public class SettingsTab
{
    public SettingsTab(string path, string name)
    {
        Path = path;
        Name = name;
    }

    public string Path { get; set; }
    public string Name { get; set; }

    public bool Matches(string path)
    {
        return path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase);
    }
}