using Avalonia;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

public static class SKMatrixExtensions
{
    public static Matrix ToMatrix(this SKMatrix matrix)
    {
        return new Matrix(
            matrix.ScaleX,
            matrix.SkewY,
            matrix.SkewX,
            matrix.ScaleY,
            matrix.TransX,
            matrix.TransY
        );
    }
}