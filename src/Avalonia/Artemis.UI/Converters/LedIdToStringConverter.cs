using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RGB.NET.Core;

namespace Artemis.UI.Converters
{
    public class LedIdToStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Enum.TryParse(typeof(LedId), value?.ToString(), true, out object parsedLedId))
                return parsedLedId;
            return LedId.Unknown1;
        }

        #endregion
    }
}