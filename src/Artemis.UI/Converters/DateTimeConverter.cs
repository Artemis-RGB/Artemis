using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Humanizer;

namespace Artemis.UI.Converters;

public class DateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            return parameter?.ToString() == "humanize" 
                ? dateTimeOffset.ToLocalTime().Humanize() 
                : dateTimeOffset.ToLocalTime().ToString(parameter?.ToString() ?? "g");
        }

        if (value is DateTime dateTime)
        {
            return parameter?.ToString() == "humanize" 
                ? dateTime.ToLocalTime().Humanize() 
                : dateTime.ToLocalTime().ToString(parameter?.ToString() ?? "g");
        }

        return value;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}