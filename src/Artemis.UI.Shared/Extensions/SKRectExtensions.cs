using Avalonia;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides utility methods when working with SkiaSharp rectangles.
/// </summary>
public static class SKRectExtensions
{
    /// <summary>
    ///     Converts the rectangle to an Avalonia <see cref="Rect" />.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    /// <returns>The resulting Avalonia <see cref="Rect" />.</returns>
    public static Rect ToRect(this SKRect rect)
    {
        return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }

    /// <summary>
    ///     Converts the integer rectangle to an Avalonia <see cref="Rect" />.
    /// </summary>
    /// <param name="rect">The integer rectangle to convert.</param>
    /// <returns>The resulting Avalonia <see cref="Rect" />.</returns>
    public static Rect ToRect(this SKRectI rect)
    {
        return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }
}