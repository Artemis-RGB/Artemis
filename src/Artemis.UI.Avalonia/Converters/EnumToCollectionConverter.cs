using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Avalonia.Converters
{
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        private static string Description(Enum value)
        {
            object[] attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute)?.Description;

            // If no description is found, the least we can do is replace underscores with spaces
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }

        private static IEnumerable<Tuple<object, object>> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select(e => new Tuple<object, object>(e, Description(e))).ToList();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetAllValuesAndDescriptions(value.GetType());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}