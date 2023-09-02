using System;

namespace Artemis.UI.Routing;

public class RouteViewModel
{
    public RouteViewModel(string name, string path, string? mathPath = null)
    {
        Path = path;
        Name = name;
        MathPath = mathPath;
    }

    public string Path { get; }
    public string Name { get; }
    public string? MathPath { get; }

    public bool Matches(string path)
    {
        return path.StartsWith(MathPath ?? Path, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}