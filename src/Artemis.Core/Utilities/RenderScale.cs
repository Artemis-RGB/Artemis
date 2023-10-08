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
        int roundX = (int) MathF.Floor(x);
        int roundY = (int) MathF.Floor(y);
        int roundWidth = (int) MathF.Ceiling(width);
        int roundHeight = (int) MathF.Ceiling(height);

        if (RenderScaleMultiplier == 1)
            return SKRectI.Create(roundX, roundY, roundWidth, roundHeight);

        return SKRectI.Create(
            roundX - roundX % RenderScaleMultiplier,
            roundY - roundY % RenderScaleMultiplier,
            roundWidth - roundWidth % RenderScaleMultiplier,
            roundHeight - roundHeight % RenderScaleMultiplier
        );
    }
}