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
            FrameworkElement element = (FrameworkElement) d;

            element.MouseUp += element_MouseUp;
        }

        private static void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement) sender;

            ICommand command = GetMouseUpCommand(element);

            command.Execute(e);
        }
    }
}