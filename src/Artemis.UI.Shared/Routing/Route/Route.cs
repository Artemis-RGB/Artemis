using System.Collections.Generic;
using System.Linq;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a route at a certain path.
/// </summary>
public class Route
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Route" /> class.
    /// </summary>
    /// <param name="path">The path of the route.</param>
    public Route(string path)
    {
        Path = path;
        Segments = path.Split('/').Select(s => new RouteSegment(s)).ToList();
    }

    /// <summary>
    ///     Gets the path of the route.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Gets the list of segments that makes up the path.
    /// </summary>
    public List<RouteSegment> Segments { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Path;
    }
}