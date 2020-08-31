using System.Windows;
using System.Windows.Media;

namespace Artemis.UI.Shared
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///     Provides utilities for navigating the visual tree
    /// </summary>
    public static class VisualTreeUtilities
    {
        /// <summary>
        ///     Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found,
        ///     a null parent is being returned.
        /// </returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        ///     Finds a parent of a given item in the visual tree.
        /// </summary>
        /// <param name="child">A child of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="parentName">x:Name or Name of parent. </param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found,
        ///     a null parent is being returned.
        /// </returns>
        public static T FindParent<T>(DependencyObject child, string parentName) where T : DependencyObject
        {
            // Get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            // We've reached the end of the tree
            if (parentObject == null)
                return null;

            // Check if the parent matches the type we're looking for
            if (!(parentObject is T parent))
                return FindParent<T>(parentObject, parentName);

            // If no name is set the first matching type is a match
            if (string.IsNullOrEmpty(parentName))
                return parent;

            // If the parent's name is set for search that must match as well
            if (parent is FrameworkElement frameworkElement && frameworkElement.Name == parentName)
                return parent;
            return FindParent<T>(parentObject, parentName);
        }
    }
}