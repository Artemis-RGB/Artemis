using System;

namespace Artemis.UI.Shared.Routing.ParameterParsers;

internal class GuidParameterParser : IRouteParameterParser
{
    /// <inheritdoc />
    public bool IsMatch(RouteSegment segment, string source)
    {
        return Guid.TryParse(source, out _);
    }

    /// <inheritdoc />
    public object GetValue(RouteSegment segment, string source)
    {
        return Guid.Parse(source);
    }
}