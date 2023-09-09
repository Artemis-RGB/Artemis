using System;
using System.Text.RegularExpressions;
using Artemis.UI.Shared.Routing.ParameterParsers;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents a segment of a route.
/// </summary>
public partial class RouteSegment
{
    private readonly IRouteParameterParser? _parameterParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteSegment"/> class.
    /// </summary>
    /// <param name="segment">The segment value.</param>
    public RouteSegment(string segment)
    {
        Segment = segment;

        Match match = ParameterRegex().Match(segment);
        if (match.Success)
        {
            Parameter = match.Groups[1].Value;
            ParameterType = match.Groups[2].Value;
            _parameterParser = GetParameterParser(ParameterType);
        }
    }

    /// <summary>
    /// Gets the segment value.
    /// </summary>
    public string Segment { get; }

    /// <summary>
    /// Gets the parameter name if the segment is a parameterized segment; otherwise <see langword="null"/>.
    /// </summary>
    public string? Parameter { get; }

    /// <summary>
    /// Gets the type of the parameter if the segment is a parameterized segment; otherwise <see langword="null"/>.
    /// </summary>
    public string? ParameterType { get; }

    /// <summary>
    /// Checks if the segment matches the provided value.
    /// </summary>
    /// <param name="value">The value to compare with the segment.</param>
    /// <returns><see langword="true"/> if the segment matches the value; otherwise, <see langword="false"/>.</returns>
    public bool IsMatch(string value)
    {
        if (_parameterParser == null)
            return Segment.Equals(value, StringComparison.InvariantCultureIgnoreCase);
        return _parameterParser.IsMatch(this, value);
    }

    /// <summary>
    /// Gets the parameter value from the provided value.
    /// </summary>
    /// <param name="value">The value from which to extract the parameter value.</param>
    /// <returns>The extracted parameter value.</returns>
    public object? GetParameter(string value)
    {
        if (_parameterParser == null)
            return null;

        return _parameterParser.GetValue(this, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Segment} (param: {Parameter ?? "none"}, type: {ParameterType ?? "N/A"})";
    }

    private IRouteParameterParser GetParameterParser(string parameterType)
    {
        return parameterType switch
        {
            "guid" => new GuidParameterParser(),
            "long" => new LongParameterParser(),
            "int" => new IntParameterParser(),
            _ => new StringParameterParser()
        };

        // Default to a string parser which just returns the segment as is
    }

    /// <summary>
    /// Gets the regular expression used to identify parameterized segments in the route.
    /// </summary>
    /// <returns>The regular expression pattern.</returns>
    [GeneratedRegex(@"\{(\w+):(\w+)\}")]
    private static partial Regex ParameterRegex();
}