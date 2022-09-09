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
        if (value is not Numeric numeric)
            return value;

        object result = Numeric.IsTypeCompatible(targetType) ? numeric.ToType(targetType, NumberFormatInfo.InvariantInfo) : value;
        return result;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new Numeric(value);
    }
}