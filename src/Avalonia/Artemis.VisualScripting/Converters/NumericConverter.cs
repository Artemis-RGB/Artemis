using System.Globalization;
using Artemis.Core;
using Avalonia.Data.Converters;

namespace Artemis.VisualScripting.Converters;

/// <summary>
///     Converts input into <see cref="Numeric" />.
/// </summary>
public class NumericConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(Numeric))
            return new Numeric(value);

        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(Numeric))
            return new Numeric(value);

        return value;
    }
}