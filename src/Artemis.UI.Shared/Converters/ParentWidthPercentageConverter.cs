using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Artemis.UI.Shared.Converters;

/// <summary>
///     Converts the width in percentage to a real number based on the width of the given parent
/// </summary>
public class ParentWidthPercentageConverter : IValueConverter
{
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not Control parent || value is not double percentage)
            return value;

        return parent.Width / 100.0 * percentage;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not Control parent || value is not double real)
            return value;

        return real / parent.Width * 100.0;
    }

    #endregion
}