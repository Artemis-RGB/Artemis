namespace Artemis.UI.Shared.Routing.ParameterParsers;

public class StringParameterParser : IRouteParameterParser
{
    /// <inheritdoc />
    public bool IsMatch(RouteSegment segment, string source)
    {
        return true;
    }

    /// <inheritdoc />
    public object GetValue(RouteSegment segment, string source)
    {
        return source;
    }
}