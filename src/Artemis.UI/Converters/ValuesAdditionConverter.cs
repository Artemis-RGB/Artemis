using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Artemis.UI.Converters;

public class ValuesAdditionConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null)
            return 0.0;
        return values.Where(v => v is double).Cast<double>().Sum();
    }
}