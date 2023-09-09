
namespace Artemis.UI.Shared.Routing.ParameterParsers;

internal class LongParameterParser : IRouteParameterParser
{
    /// <inheritdoc />
    public bool IsMatch(RouteSegment segment, string source)
    {
        return long.TryParse(source, out _);
    }

    /// <inheritdoc />
    public object GetValue(RouteSegment segment, string source)
    {
        return long.Parse(source);
    }
}