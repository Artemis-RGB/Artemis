using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Artemis.UI.Behaviors
{
    public class InputBindingBehavior
    {
        private static List<Tuple<FrameworkElement, Window, InputBinding>> _movedInputBindings = new List<Tuple<FrameworkElement, Window, InputBinding>>();

        public static readonly DependencyProperty PropagateInputBindingsToWindowProperty =
            DependencyProperty.RegisterAttached("PropagateInputBindingsToWindow", typeof(bool), typeof(InputBindingBehavior),
                new PropertyMetadata(false, OnPropagateInputBindingsToWindowChanged));

        public static bool GetPropagateInputBindingsToWindow(FrameworkElement obj)
        {
            return (bool) obj.GetValue(PropagateInputBindingsToWindowProperty);
        }

        public static void SetPropagateInputBindingsToWindow(FrameworkElement obj, bool value)
        {
            obj.SetValue(PropagateInputBindingsToWindowProperty, value);
        }

        private static void OnPropagateInputBindingsToWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement) d).Loaded += OnLoaded;
            ((FrameworkElement) d).Unloaded += OnUnloaded;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement) sender;

            var window = Window.GetWindow(frameworkElement);
            if (window == null) return;

            // Move input bindings from the FrameworkElement to the window.
            for (var i = frameworkElement.InputBindings.Count - 1; i >= 0; i--)
            {
                var inputBinding = frameworkElement.InputBindings[i];
                window.InputBindings.Add(inputBinding);
                frameworkElement.InputBindings.Remove(inputBinding);

                _movedInputBindings.Add(new Tuple<FrameworkElement, Window, InputBinding>(frameworkElement, window, inputBinding));
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement) sender;

            var toRemove = _movedInputBindings.Where(m => m.Item1 == frameworkElement).ToList();
            foreach (var (_, window, inputBinding) in toRemove)
            {
                if (window.InputBindings.Contains(inputBinding))
                    window.InputBindings.Remove(inputBinding);
            }

            _movedInputBindings = _movedInputBindings.Where(b => b.Item1 != frameworkElement).ToList();
        }
    }
}