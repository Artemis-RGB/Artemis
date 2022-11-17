using System;
using System.Runtime.CompilerServices;

namespace Artemis.Core.ColorScience;

//HACK DarthAffe 17.11.2022: Due to the high amount of inlined code this is not supposed to be used outside the ColorSorter!
internal static class Cie94
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

        float deltaH = Pow2(deltaA) + Pow2(deltaB) - Pow2(deltaC);
        deltaH = deltaH < 0f ? 0f : MathF.Sqrt(deltaH);

        float sc = 1.0f + K1 * c1;
        float sh = 1.0f + K2 * c1;

        float i = Pow2(deltaL / (KL * SL)) + Pow2(deltaC / (KC * sc)) + Pow2(deltaH / (KH * sh));

        return i < 0f ? 0f : MathF.Sqrt(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pow2(in float x) => x * x;
}
