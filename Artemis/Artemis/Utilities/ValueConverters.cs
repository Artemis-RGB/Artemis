using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Artemis.Utilities
{
    /// <summary>
    ///     Fredrik Hedblad - http://stackoverflow.com/a/3987099/5015269
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myEnum = (Enum) value;
            var description = GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        private string GetEnumDescription(Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            var attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            var attrib = attribArray[0] as DescriptionAttribute;
            return attrib?.Description;
        }
    }
}