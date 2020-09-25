﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;

namespace Artemis.UI.Converters
{
    public class PropertyTreeMarginConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeGroupViewModel treeGroupViewModel)
                return new Thickness(Length * treeGroupViewModel.GetDepth(), 0, 0, 0);
            if (value is ITreePropertyViewModel treePropertyViewModel)
                return new Thickness(Length * treePropertyViewModel.GetDepth(), 0, 0, 0);

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}