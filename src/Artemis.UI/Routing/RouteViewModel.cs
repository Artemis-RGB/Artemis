using System;

namespace Artemis.UI.Routing;

public class RouteViewModel
{
    public RouteViewModel(string name, string path, string? matchPath = null)
    {
        Path = path;
        Name = name;
        MatchPath = matchPath;
    }

    public string Path { get; }
    public string Name { get; }
    public string? MatchPath { get; }

    public bool Matches(string path)
    {
        return path.StartsWith(MatchPath ?? Path, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}