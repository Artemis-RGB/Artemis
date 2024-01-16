using System.Globalization;
using Artemis.Core;
using Avalonia.Data.Converters;
using Newtonsoft.Json;

namespace Artemis.VisualScripting.Converters;

/// <summary>
///     Converts input into <see cref="Numeric" />.
/// </summary>
public class JsonConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return JsonConvert.SerializeObject(value, Formatting.Indented);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? json = value?.ToString();
        return json == null ? null : JsonConvert.DeserializeObject(json, targetType);
    }
}