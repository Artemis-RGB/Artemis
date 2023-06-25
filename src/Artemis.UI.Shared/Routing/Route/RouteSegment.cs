using System;
using System.Text.RegularExpressions;
using Artemis.UI.Shared.Routing.ParameterParsers;

namespace Artemis.UI.Shared.Routing;

public partial class RouteSegment
{
    private readonly IRouteParameterParser? _parameterParser;

    public RouteSegment(string segment)
    {
        Segment = segment;

        Match match = ParameterRegex().Match(segment);
        if (match.Success)
        {
            Parameter = match.Groups[0].Value;
            ParameterType = match.Groups[1].Value;
            _parameterParser = GetParameterParser(ParameterType);
        }
    }

    public string Segment { get; }
    public string? Parameter { get; }
    public string? ParameterType { get; }

    public bool IsMatch(string value)
    {
        if (_parameterParser == null)
            return Segment.Equals(value, StringComparison.InvariantCultureIgnoreCase);
        return _parameterParser.IsMatch(this, value);
    }

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
        if (parameterType == "guid")
            return new GuidParameterParser();

        // Default to a string parser which just returns the segment as is
        return new StringParameterParser();
    }

    [GeneratedRegex(@"\{(\w+):(\w+)\}")]
    private static partial Regex ParameterRegex();
}