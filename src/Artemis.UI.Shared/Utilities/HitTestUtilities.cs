using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Artemis.UI.Shared.Utilities
{
    public static class HitTestUtilities
    {
        /// <summary>
        ///     Runs a hit test on children of the container within the rectangle matching all elements that have a data context of
        ///     <see cref="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="rectangleGeometry"></param>
        /// <returns></returns>
        public static List<T> GetHitViewModels<T>(Visual container, RectangleGeometry rectangleGeometry)
        {
            var result = new List<T>();
            var hitTestParams = new GeometryHitTestParameters(rectangleGeometry);
            var resultCallback = new HitTestResultCallback(r => HitTestResultBehavior.Continue);
            var filterCallback = new HitTestFilterCallback(e =>
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