using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.VisualTree;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides extension methods for Avalonia's <see cref="Visual" /> type
/// </summary>
public static class VisualExtensions
{
    /// <summary>
    ///     Returns a recursive list of all visual children of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type the children should have.</typeparam>
    /// <param name="root">The root visual at which to start searching.</param>
    /// <returns>A recursive list of all visual children of type <typeparamref name="T" />.</returns>
    public static List<T> GetVisualChildrenOfType<T>(this Visual root)
    {
        List<T> result = new();

        List<Visual>? visualChildren = root.GetVisualChildren()?.ToList();
        if (visualChildren == null || !visualChildren.Any())
            return result;

        foreach (Visual visualChild in visualChildren)
        {
            if (visualChild is T toFind)
                result.Add(toFind);

            result.AddRange(GetVisualChildrenOfType<T>(visualChild));
        }

        return result;
    }

    /// <summary>
    ///     Returns a recursive list of all visual children with a data context of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of data context the children should have.</typeparam>
    /// <param name="root">The root visual at which to start searching.</param>
    /// <returns>A recursive list of all visual children with a data context of type <typeparamref name="T" />.</returns>
    public static List<T> GetVisualChildrenOfDataContextType<T>(this Visual root)
    {
        List<T> result = new();

        List<Visual>? visualChildren = root.GetVisualChildren()?.ToList();
        if (visualChildren == null || !visualChildren.Any())
            return result;

        foreach (Visual visualChild in visualChildren)
        {
            if (visualChild is IDataContextProvider dataContextProvider && dataContextProvider.DataContext is T toFind)
                result.Add(toFind);

            result.AddRange(GetVisualChildrenOfType<T>(visualChild));
        }

        return result;
    }
}