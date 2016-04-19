using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Artemis.Models.Profiles;
using Artemis.Utilities.ParentChild;

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

    public class LayerOrderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList collection;
            if (value is ChildItemCollection<LayerModel, LayerModel>)
                collection = ((ChildItemCollection<LayerModel, LayerModel>) value).ToList();
            else
                collection = (IList) value;

            var view = new ListCollectionView(collection);
            var sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);
            view.SortDescriptions.Add(sort);

            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}