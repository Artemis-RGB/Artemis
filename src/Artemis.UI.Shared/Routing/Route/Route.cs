using System.Collections.Generic;
using System.Linq;

namespace Artemis.UI.Shared.Routing;

public class Route
{
    public Route(string path)
    {
        Path = path;
        Segments = path.Split('/').Select(s => new RouteSegment(s)).ToList();
    }

    public string Path { get; }
    public List<RouteSegment> Segments { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Path;
    }
}