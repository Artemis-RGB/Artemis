namespace Artemis.UI.Shared.Routing.ParameterParsers;

/// <summary>
/// Represents a contract for parsing route parameters.
/// </summary>
public interface IRouteParameterParser
{
    /// <summary>
    /// Checks if the given segment matches the provided source.
    /// </summary>
    /// <param name="segment">The route segment to match.</param>
    /// <param name="source">The source value to match against the route segment.</param>
    /// <returns><see langword="true"/> if the segment matches the source; otherwise, <see langword="false"/>.</returns>
    bool IsMatch(RouteSegment segment, string source);

    /// <summary>
    /// Gets the parameter value from the provided source value.
    /// </summary>
    /// <param name="segment">The route segment containing the parameter information.</param>
    /// <param name="source">The source value from which to extract the parameter value.</param>
    /// <returns>The extracted parameter value.</returns>
    object GetValue(RouteSegment segment, string source);
}