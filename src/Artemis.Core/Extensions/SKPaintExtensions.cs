using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     A static class providing <see cref="SKPaint" /> extensions
/// </summary>
public static class SKPaintExtensions
{
    /// <summary>
    ///     Disposes the paint and its disposable properties such as shaders and filters.
    /// </summary>
    /// <param name="paint">The pain to dispose.</param>
    public static void DisposeSelfAndProperties(this SKPaint paint)
    {
        paint.ImageFilter?.Dispose();
        paint.ColorFilter?.Dispose();
        paint.MaskFilter?.Dispose();
        paint.Shader?.Dispose();
        paint.Dispose();
    }
}