using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Artemis.UI.Shared.Converters;

/// <summary>
///     Converts the width in percentage to a real number based on the width of the given parent
/// </summary>
public class WidthNormalizedConverter : IMultiValueConverter
{
    #region Implementation of IMultiValueConverter

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? first = values.FirstOrDefault();
        object? second = values.Skip(1).FirstOrDefault();
        if (first is float value && second is double totalWidth)
            return totalWidth / 1.0 * value;

        return 0.0;
    }

    #endregion
}