using System;

namespace Artemis.UI.Shared.Routing.ParameterParsers;

public class GuidParameterParser : IRouteParameterParser
{
    /// <inheritdoc />
    public bool IsMatch(RouteSegment segment, string source)
    {
        return Guid.TryParse(segment.Parameter, out _);
    }

    /// <inheritdoc />
    public object GetValue(RouteSegment segment, string source)
    {
        return Guid.Parse(source);
    }
}