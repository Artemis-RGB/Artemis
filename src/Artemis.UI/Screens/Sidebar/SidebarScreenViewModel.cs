using System;
using Artemis.UI.Shared;
using Material.Icons;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarScreenViewModel : ViewModelBase
{
    public SidebarScreenViewModel(MaterialIconKind icon, string displayName, string path)
    {
        Icon = icon;
        Path = path;
        DisplayName = displayName;
    }

    public MaterialIconKind Icon { get; }
    public string Path { get; }

    public bool Matches(string? path)
    {
        if (path == null)
            return false;
        return path.StartsWith(Path, StringComparison.InvariantCultureIgnoreCase);
    }
}