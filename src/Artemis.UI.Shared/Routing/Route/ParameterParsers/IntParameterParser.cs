
namespace Artemis.UI.Shared.Routing.ParameterParsers;

internal class IntParameterParser : IRouteParameterParser
{
    /// <inheritdoc />
    public bool IsMatch(RouteSegment segment, string source)
    {
        return int.TryParse(source, out _);
    }

    /// <inheritdoc />
    public object GetValue(RouteSegment segment, string source)
    {
        return int.Parse(source);
    }
}