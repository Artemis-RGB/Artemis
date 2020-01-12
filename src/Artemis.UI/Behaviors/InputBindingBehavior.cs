using System.Windows;

namespace Artemis.UI.Behaviors
{
    public class InputBindingBehavior
    {
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
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Loaded -= OnLoaded;

            var window = Window.GetWindow(frameworkElement);
            if (window == null) return;

            // Move input bindings from the FrameworkElement to the window.
            for (var i = frameworkElement.InputBindings.Count - 1; i >= 0; i--)
            {
                var inputBinding = frameworkElement.InputBindings[i];
                window.InputBindings.Add(inputBinding);
                frameworkElement.InputBindings.Remove(inputBinding);
            }
        }
    }
}