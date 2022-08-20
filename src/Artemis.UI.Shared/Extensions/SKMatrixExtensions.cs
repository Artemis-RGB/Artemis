using Avalonia;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides utility methods when working with SkiaSharp matrices.
/// </summary>
public static class SKMatrixExtensions
{
    /// <summary>
    ///     Converts the matrix to an Avalonia <see cref="Matrix" />.
    /// </summary>
    /// <param name="matrix">The matrix to convert.</param>
    /// <returns>The resulting Avalonia <see cref="Matrix" />.</returns>
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