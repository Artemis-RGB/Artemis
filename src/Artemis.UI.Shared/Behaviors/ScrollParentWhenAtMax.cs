using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Artemis.UI.Shared
{
    /// <summary>
    /// A behavior that makes a scroll viewer bubble up its events if it is at its scroll limit
    /// </summary>
    public class ScrollParentWhenAtMax : Behavior<FrameworkElement>
    {
        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += PreviewMouseWheel;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= PreviewMouseWheel;
            base.OnDetaching();
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer? scrollViewer = GetVisualChild<ScrollViewer>(AssociatedObject);
            if (scrollViewer == null)
                return;
            double scrollPos = scrollViewer.ContentVerticalOffset;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (scrollPos == scrollViewer.ScrollableHeight && e.Delta < 0 || scrollPos == 0 && e.Delta > 0)
            {
                e.Handled = true;
                MouseWheelEventArgs e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                e2.RoutedEvent = UIElement.MouseWheelEvent;
                AssociatedObject.RaiseEvent(e2);
            }
        }

        private static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T? child = default;

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual) VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }

            return child;
        }
    }
}