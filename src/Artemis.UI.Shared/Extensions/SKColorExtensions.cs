using Avalonia.Media;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="SKColor"/> type.
/// </summary>
public static class SKColorExtensions
{
    /// <summary>
    /// Converts a SkiaSharp <see cref="SKColor"/> to an Avalonia <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The resulting color.</returns>
    public static Color ToColor(this SKColor color)
    {
        return new Color(color.Alpha, color.Red, color.Green, color.Blue);
    }
}