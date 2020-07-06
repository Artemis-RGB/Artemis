using System.Windows;
using System.Windows.Input;

namespace Artemis.UI.Behaviors
{
    public class MouseBehavior
    {
        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand", typeof(ICommand),
                typeof(MouseBehavior), new FrameworkPropertyMetadata(
                    MouseUpCommandChanged));

        public static void SetMouseUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(MouseUpCommandProperty, value);
        }

        public static ICommand GetMouseUpCommand(UIElement element)
        {
            return (ICommand) element.GetValue(MouseUpCommandProperty);
        }

        private static void MouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement) d;

            element.MouseUp += element_MouseUp;
        }

        private static void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement) sender;

            var command = GetMouseUpCommand(element);

            command.Execute(e);
        }
    }
}