using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Artemis.UI.Shared.Converters;

/// <summary>
/// Converts any object to string by calling its ToString implementation, seems Avalonia doesn't do this
/// </summary>
public class ToStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}