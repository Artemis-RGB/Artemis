using System;
using System.Windows;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;
using MahApps.Metro.Controls;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Helper
{
    public class ToggleSwitchButtonEnumFlagHelper
    {
        #region DependencyProperties

        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty FlagsProperty = DependencyProperty.RegisterAttached(
            "Flags", typeof(Enum), typeof(ToggleSwitchButtonEnumFlagHelper),
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
            "Value", typeof(Enum), typeof(ToggleSwitchButtonEnumFlagHelper), new PropertyMetadata(default(Enum), ValueChanged));

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
            UpdateTarget(dependencyObject as ToggleSwitchButton, dependencyPropertyChangedEventArgs.NewValue as Enum);
        }

        private static void ValueChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var toggleSwitchButton = dependencyObject as ToggleSwitchButton;
            if (toggleSwitchButton == null)
                return;

            toggleSwitchButton.Checked -= UpdateSource;
            toggleSwitchButton.Unchecked -= UpdateSource;

            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                toggleSwitchButton.Checked += UpdateSource;
                toggleSwitchButton.Unchecked += UpdateSource;
            }

            UpdateTarget(toggleSwitchButton, GetFlags(toggleSwitchButton));
        }

        private static void UpdateTarget(ToggleSwitchButton toggleSwitchButton, Enum flags)
        {
            if (toggleSwitchButton == null)
                return;

            var value = GetValue(toggleSwitchButton);
            toggleSwitchButton.IsChecked = value != null && (flags?.HasFlag(value) ?? false);
        }

        private static void UpdateSource(object sender, RoutedEventArgs routedEventArgs)
        {
            var toggleSwitchButton = sender as ToggleSwitchButton;
            if (toggleSwitchButton == null)
                return;

            var flags = GetFlags(toggleSwitchButton);
            var value = GetValue(toggleSwitchButton);
            if (value == null)
                return;

            if (toggleSwitchButton.IsChecked ?? false)
                SetFlags(toggleSwitchButton, flags == null ? value : flags.SetFlag(value, true, flags.GetType()));
            else
                SetFlags(toggleSwitchButton, flags?.SetFlag(value, false, flags.GetType()));
        }

        #endregion
    }
}
