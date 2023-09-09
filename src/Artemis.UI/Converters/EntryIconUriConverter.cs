using System;
using System.Globalization;
using Artemis.WebClient.Workshop;
using Avalonia.Data.Converters;

namespace Artemis.UI.Converters;

public class EntryIconUriConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return $"{WorkshopConstants.WORKSHOP_URL}/entries/{value}/icon";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}