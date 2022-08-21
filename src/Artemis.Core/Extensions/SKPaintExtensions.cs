using SkiaSharp;

namespace Artemis.Core;

internal static class SKPaintExtensions
{
    internal static void DisposeSelfAndProperties(this SKPaint paint)
    {
        paint.ImageFilter?.Dispose();
        paint.ColorFilter?.Dispose();
        paint.MaskFilter?.Dispose();
        paint.Shader?.Dispose();
        paint.Dispose();
    }
}