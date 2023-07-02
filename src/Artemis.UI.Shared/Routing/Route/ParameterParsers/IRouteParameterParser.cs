namespace Artemis.UI.Shared.Routing.ParameterParsers;

public interface IRouteParameterParser
{
    bool IsMatch(RouteSegment segment, string source);
    object GetValue(RouteSegment segment, string source);
}