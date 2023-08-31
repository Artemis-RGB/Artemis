using System;

namespace Artemis.UI.Routing;

public class RouteViewModel
{
    public RouteViewModel(string path, string name)
    {
        Path = path;
        Name = name;
    }

    public string Path { get; }
    public string Name { get; }

    public bool Matches(string path)
    {
        return path.StartsWith(Path, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}