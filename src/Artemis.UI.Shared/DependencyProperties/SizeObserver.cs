using System.Windows;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides a dependency property that can observe the size of an element and apply it to bindings
    /// </summary>
    public static class SizeObserver
    {
        /// <summary>
        ///     Gets or sets whether the element should be observed
        /// </summary>
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(SizeObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        /// <summary>
        ///     Gets or sets the observed width of the element
        /// </summary>
        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(SizeObserver));

        /// <summary>
        ///     Gets or sets the observed height of the element
        /// </summary>
        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(SizeObserver));

        /// <summary>
        ///     Gets whether the provided <paramref name="frameworkElement" /> is being observed
        /// </summary>
        public static bool GetObserve(FrameworkElement frameworkElement)
        {
            return (bool) frameworkElement.GetValue(ObserveProperty);
        }

        /// <summary>
        ///     Sets whether the provided <paramref name="frameworkElement" /> is being observed
        /// </summary>
        public static void SetObserve(FrameworkElement frameworkElement, bool observe)
        {
            frameworkElement.SetValue(ObserveProperty, observe);
        }

        /// <summary>
        ///     Gets the observed width of the the provided <paramref name="frameworkElement" />
        /// </summary>
        public static double GetObservedWidth(FrameworkElement frameworkElement)
        {
            return (double) frameworkElement.GetValue(ObservedWidthProperty);
        }

        /// <summary>
        ///     Sets the observed width of the the provided <paramref name="frameworkElement" />
        /// </summary>
        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth)
        {
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        /// <summary>
        ///     Gets the observed height of the the provided <paramref name="frameworkElement" />
        /// </summary>
        public static double GetObservedHeight(FrameworkElement frameworkElement)
        {
            return (double) frameworkElement.GetValue(ObservedHeightProperty);
        }

        /// <summary>
        ///     Sets the observed height of the the provided <paramref name="frameworkElement" />
        /// </summary>
        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight)
        {
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement) dependencyObject;

            if ((bool) e.NewValue)
            {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateObservedSizesForFrameworkElement((FrameworkElement) sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement)
        {
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }
    }
}