using RGB.NET.Core;
using SkiaSharp;
using System.Runtime.CompilerServices;
using static System.MathF;

namespace Artemis.Core.ColorScience;

internal readonly struct LabColor
{
    #region Constants

    private const float LAB_REFERENCE_X = 0.95047f;
    private const float LAB_REFERENCE_Y = 1.0f;
    private const float LAB_REFERENCE_Z = 1.08883f;

    #endregion

    #region Properties & Fields

    public readonly float L;
    public readonly float A;
    public readonly float B;

    #endregion

    #region Constructors

    public LabColor(SKColor color)
        : this(color.Red.GetPercentageFromByteValue(),
               color.Green.GetPercentageFromByteValue(),
               color.Blue.GetPercentageFromByteValue())
    { }

    public LabColor(float r, float g, float b)
    {
        (L, A, B) = RgbToLab((r, g, b));
    }

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (float x, float y, float z) RgbToXyz((float r, float g, float b) rgb)
    {
        (float r, float g, float b) = rgb;

        if (r > 0.04045f)
            r = Pow((r + 0.055f) / 1.055f, 2.4f);
        else
            r /= 12.92f;

        if (g > 0.04045)
            g = Pow((g + 0.055f) / 1.055f, 2.4f);
        else
            g /= 12.92f;

        if (b > 0.04045)
            b = Pow((b + 0.055f) / 1.055f, 2.4f);
        else
            b /= 12.92f;

        float x = (r * 0.4124f) + (g * 0.3576f) + (b * 0.1805f);
        float y = (r * 0.2126f) + (g * 0.7152f) + (b * 0.0722f);
        float z = (r * 0.0193f) + (g * 0.1192f) + (b * 0.9505f);

        return (x, y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (float l, float a, float b) XyzToLab((float x, float y, float z) xyz)
    {
        const float POWER = 1.0f / 3.0f;
        const float OFFSET = 16.0f / 116.0f;

        float x = xyz.x / LAB_REFERENCE_X;
        float y = xyz.y / LAB_REFERENCE_Y;
        float z = xyz.z / LAB_REFERENCE_Z;

        if (x > 0.008856f)
            x = Pow(x, POWER);
        else
            x = (7.787f * x) + OFFSET;

        if (y > 0.008856f)
            y = Pow(y, POWER);
        else
            y = (7.787f * y) + OFFSET;

        if (z > 0.008856f)
            z = Pow(z, POWER);
        else
            z = (7.787f * z) + OFFSET;

        float l = (116f * y) - 16f;
        float a = 500f * (x - y);
        float b = 200f * (y - z);

        return (l, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (float l, float a, float b) RgbToLab((float r, float g, float b) rgb) => XyzToLab(RgbToXyz(rgb));

    #endregion
}
