using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core;

public static class ColorUtilities
{
    private const int STACK_ALLOC_LIMIT = 1024;
    private readonly record struct SortColor(SKColor RGB, LabColor Lab);

    public static void Sort(in Span<SKColor> colors, SKColor startColor = new())
    {
        LabColor referenceColor = new(startColor);

        if (colors.Length < STACK_ALLOC_LIMIT)
        {
            Span<SortColor> sortColors = stackalloc SortColor[colors.Length];
            Sort(colors, sortColors, referenceColor);
        }
        else
        {
            SortColor[] sortColorArray = ArrayPool<SortColor>.Shared.Rent(colors.Length);
            Span<SortColor> sortColors = sortColorArray.AsSpan(0, colors.Length);
            Sort(colors, sortColors, referenceColor);
            ArrayPool<SortColor>.Shared.Return(sortColorArray);
        }
    }

    private static void Sort(in Span<SKColor> colors, Span<SortColor> sortColors, LabColor referenceColor)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            SKColor color = colors[i];
            sortColors[i] = new SortColor(color, new LabColor(color));
        }

        for (int i = 0; i < colors.Length; i++)
        {
            float closestDistance = float.MaxValue;
            int closestIndex = -1;
            for (int j = 0; j < sortColors.Length; j++)
            {
                float distance = CIE94.ComputeDifference(sortColors[j].Lab, referenceColor);
                if (distance == 0f)
                {
                    closestIndex = j;
                    break;
                }

                if (distance < closestDistance)
                {
                    closestIndex = j;
                    closestDistance = distance;
                }
            }

            SortColor closestColor = sortColors[closestIndex];
            colors[i] = closestColor.RGB;
            referenceColor = closestColor.Lab;

            sortColors[closestIndex] = sortColors[^1];
            sortColors = sortColors[..^1];
        }
    }
}

internal static class CIE94
{
    const float KL = 1.0f;
    const float K1 = 0.045f;
    const float K2 = 0.015f;
    const float SL = 1.0f;
    const float KC = 1.0f;
    const float KH = 1.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ComputeDifference(in LabColor x, in LabColor y)
    {
        float deltaL = x.L - y.L;
        float deltaA = x.A - y.A;
        float deltaB = x.B - y.B;

        float c1 = MathF.Sqrt(Pow2(x.A) + Pow2(x.B));
        float c2 = MathF.Sqrt(Pow2(y.A) + Pow2(y.B));
        float deltaC = c1 - c2;

        float deltaH = (Pow2(deltaA) + Pow2(deltaB)) - Pow2(deltaC);
        deltaH = deltaH < 0f ? 0f : MathF.Sqrt(deltaH);

        float sc = 1.0f + (K1 * c1);
        float sh = 1.0f + (K2 * c1);

        float i = Pow2(deltaL / (KL * SL)) + Pow2(deltaC / (KC * sc)) + Pow2(deltaH / (KH * sh));

        return i < 0f ? 0f : MathF.Sqrt(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow2(in float x) => x * x;
}

internal readonly struct LabColor
{
    public readonly float L;
    public readonly float A;
    public readonly float B;

    public LabColor(SKColor color)
    {
        var lab = RGB.NET.Core.LabColor.GetLab(new RGB.NET.Core.Color(color.Red, color.Green, color.Blue));
        L = lab.l;
        A = lab.a;
        B = lab.b;
    }
}