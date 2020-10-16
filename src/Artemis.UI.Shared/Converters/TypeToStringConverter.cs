using System;
using System.Globalization;
using System.Windows.Data;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Converts <see cref="T:System.Type" /> into <see cref="T:System.String" />.
    /// </summary>
    public class TypeToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool humanizeProvided = bool.TryParse(parameter?.ToString(), out bool humanize);
            if (value is Type type)
                return type.GetDisplayName(humanizeProvided && humanize);

            return value?.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}