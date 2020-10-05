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
            List<T> result = new List<T>();
            GeometryHitTestParameters hitTestParams = new GeometryHitTestParameters(rectangleGeometry);
            HitTestResultCallback resultCallback = new HitTestResultCallback(r => HitTestResultBehavior.Continue);
            HitTestFilterCallback filterCallback = new HitTestFilterCallback(e =>
            {
                if (e is FrameworkElement fe && fe.DataContext is T context && !result.Contains(context))
                    result.Add(context);
                return HitTestFilterBehavior.Continue;
            });
            VisualTreeHelper.HitTest(container, filterCallback, resultCallback, hitTestParams);

            return result;
        }
    }
}