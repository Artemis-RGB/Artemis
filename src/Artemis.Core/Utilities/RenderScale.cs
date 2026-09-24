using System;
using SkiaSharp;

namespace Artemis.Core;

internal static class RenderScale
{
    internal static int RenderScaleMultiplier { get; private set; } = 2;

    internal static event EventHandler? RenderScaleMultiplierChanged;

    internal static void SetRenderScaleMultiplier(int renderScaleMultiplier)
    {
        RenderScaleMultiplier = renderScaleMultiplier;
        RenderScaleMultiplierChanged?.Invoke(null, EventArgs.Empty);
    }

    internal static SKRectI CreateScaleCompatibleRect(float x, float y, float width, float height)
    {
        int multiplier = RenderScaleMultiplier;
        if (multiplier == 1)
            return SKRectI.Create((int) MathF.Floor(x), (int) MathF.Floor(y), (int) MathF.Ceiling(width), (int) MathF.Ceiling(height));

        int left = AlignStart(x, width, multiplier);
        int top = AlignStart(y, height, multiplier);
        int right = AlignEnd(x, width, multiplier, left);
        int bottom = AlignEnd(y, height, multiplier, top);
        return SKRectI.Create(left, top, right - left, bottom - top);
    }

    private static int AlignStart(float start, float length, int multiplier)
    {
        int firstFullCell = (int) MathF.Ceiling(start / multiplier) * multiplier;
        int lastFullCellEnd = (int) MathF.Floor((start + length) / multiplier) * multiplier;

        // A thin LED may not contain a whole render pixel. Keep the cell containing
        // its center instead of rounding its width or height down to zero.
        return firstFullCell < lastFullCellEnd
            ? firstFullCell
            : (int) MathF.Floor((start + length / 2) / multiplier) * multiplier;
    }

    private static int AlignEnd(float start, float length, int multiplier, int alignedStart)
    {
        int lastFullCellEnd = (int) MathF.Floor((start + length) / multiplier) * multiplier;
        return Math.Max(lastFullCellEnd, alignedStart + multiplier);
    }
}
