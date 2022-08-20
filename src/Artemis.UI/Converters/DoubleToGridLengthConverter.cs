using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Artemis.UI.Converters;

public class DoubleToGridLengthConverter : IValueConverter
{
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
            return new GridLength(doubleValue, GridUnitType.Pixel);
        return new GridLength(1, GridUnitType.Star);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GridLength gridLength)
            return gridLength.Value;
        return 0.0;
    }

    #endregion
}