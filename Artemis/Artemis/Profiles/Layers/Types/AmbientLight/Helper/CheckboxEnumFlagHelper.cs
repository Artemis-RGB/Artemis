using System;
using System.Windows;
using System.Windows.Controls;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Helper
{
    public class CheckboxEnumFlagHelper
    {
        #region DependencyProperties

        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty FlagsProperty = DependencyProperty.RegisterAttached(
            "Flags", typeof(Enum), typeof(CheckboxEnumFlagHelper),
            new FrameworkPropertyMetadata(default(Enum), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                FlagsChanged));

        public static void SetFlags(DependencyObject element, Enum value)
        {
            element.SetValue(FlagsProperty, value);
        }

        public static Enum GetFlags(DependencyObject element)
        {
            return (Enum) element.GetValue(FlagsProperty);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            "Value", typeof(Enum), typeof(CheckboxEnumFlagHelper), new PropertyMetadata(default(Enum), ValueChanged));

        public static void SetValue(DependencyObject element, Enum value)
        {
            element.SetValue(ValueProperty, value);
        }

        public static Enum GetValue(DependencyObject element)
        {
            return (Enum) element.GetValue(ValueProperty);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region Methods

        private static void FlagsChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            UpdateTarget(dependencyObject as CheckBox, dependencyPropertyChangedEventArgs.NewValue as Enum);
        }

        private static void ValueChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var checkbox = dependencyObject as CheckBox;
            if (checkbox == null) return;

            checkbox.Checked -= UpdateSource;
            checkbox.Unchecked -= UpdateSource;

            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                checkbox.Checked += UpdateSource;
                checkbox.Unchecked += UpdateSource;
            }

            UpdateTarget(checkbox, GetFlags(checkbox));
        }

        private static void UpdateTarget(CheckBox checkbox, Enum flags)
        {
            if (checkbox == null) return;

            var value = GetValue(checkbox);
            checkbox.IsChecked = value != null && (flags?.HasFlag(value) ?? false);
        }

        private static void UpdateSource(object sender, RoutedEventArgs routedEventArgs)
        {
            var checkbox = sender as CheckBox;
            if (checkbox == null) return;

            var flags = GetFlags(checkbox);
            var value = GetValue(checkbox);
            if (value == null) return;

            if (checkbox.IsChecked ?? false)
                SetFlags(checkbox, flags == null ? value : flags.SetFlag(value, true, flags.GetType()));
            else
                SetFlags(checkbox, flags?.SetFlag(value, false, flags.GetType()));
        }

        #endregion
    }
}