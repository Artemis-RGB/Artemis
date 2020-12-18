using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides utilities for running hit tests on visual elements
    /// </summary>
    public static class HitTestUtilities
    {
        /// <summary>
        ///     Runs a hit test on children of the container within the rectangle matching all elements that have a data context of
        ///     T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="rectangleGeometry"></param>
        /// <returns></returns>
        public static List<T> GetHitViewModels<T>(Visual container, RectangleGeometry rectangleGeometry)
        {
            List<T> result = new();
            GeometryHitTestParameters hitTestParams = new(rectangleGeometry);
            
            HitTestResultBehavior ResultCallback(HitTestResult r) => HitTestResultBehavior.Continue;
            HitTestFilterBehavior FilterCallback(DependencyObject e)
            {
                if (e is FrameworkElement fe && fe.DataContext is T context && !result.Contains(context)) result.Add(context);
                return HitTestFilterBehavior.Continue;
            }

            VisualTreeHelper.HitTest(container, FilterCallback, ResultCallback, hitTestParams);

            return result;
        }
    }
}