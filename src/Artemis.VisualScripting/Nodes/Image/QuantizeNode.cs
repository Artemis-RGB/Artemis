using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Artemis.Core;
using Artemis.Core.Services;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Image;

[Node("Quantize", "Quantizes the image into key-colors", "Image", InputType = typeof(SKImage), OutputType = typeof(SKColor))]
public class QuantizeNode : Node
{
    #region Properties & Fields
    
    public InputPin<SKImage> Image { get; set; }

    public OutputPin<SKColor> Vibrant { get; set; }
    public OutputPin<SKColor> Muted { get; set; }
    public OutputPin<SKColor> DarkVibrant { get; set; }
    public OutputPin<SKColor> DarkMuted { get; set; }
    public OutputPin<SKColor> LightVibrant { get; set; }
    public OutputPin<SKColor> LightMuted { get; set; }

    #endregion

    #region Constructors

    public QuantizeNode()
        : base("Quantize", "Quantizes the image into key-colors")
    {
        Image = CreateInputPin<SKImage>("Image");

        Vibrant = CreateOutputPin<SKColor>("Vibrant");
        Muted = CreateOutputPin<SKColor>("Muted");
        DarkVibrant = CreateOutputPin<SKColor>("DarkVibrant");
        DarkMuted = CreateOutputPin<SKColor>("DarkMuted");
        LightVibrant = CreateOutputPin<SKColor>("LightVibrant");
        LightMuted = CreateOutputPin<SKColor>("LightMuted");
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        if (Image.Value == null) return;

        using SKBitmap bitmap = SKBitmap.FromImage(Image.Value);
        SKColor[] colorPalette = ColorQuantizer.Quantize(bitmap.Pixels, 32); //TODO DarthAffe 18.08.2022: Palette-Size as input
        ColorSwatch swatch = ColorQuantizer.FindAllColorVariations(colorPalette, true);

        Vibrant.Value = swatch.Vibrant;
        Muted.Value = swatch.Muted;
        DarkVibrant.Value = swatch.DarkVibrant;
        DarkMuted.Value = swatch.DarkMuted;
        LightVibrant.Value = swatch.LightVibrant;
        LightMuted.Value = swatch.LightMuted;
    }

    #endregion
}

#region Quantizer //TODO DarthAffe 18.08.2022: external project?

public static class ColorQuantizer
{
    public static SKColor[] Quantize(in Span<SKColor> colors, int amount)
    {
        if ((amount & (amount - 1)) != 0)
            throw new ArgumentException("Must be power of two", nameof(amount));

        Queue<ColorCube> cubes = new(amount);
        cubes.Enqueue(new ColorCube(colors, 0, colors.Length, SortTarget.None));

        while (cubes.Count < amount)
        {
            ColorCube cube = cubes.Dequeue();

            if (cube.TrySplit(colors, out ColorCube? a, out ColorCube? b))
            {
                cubes.Enqueue(a);
                cubes.Enqueue(b);
            }
        }

        SKColor[] result = new SKColor[cubes.Count];
        int i = 0;
        foreach (ColorCube colorCube in cubes)
            result[i++] = colorCube.GetAverageColor(colors);

        return result;
    }

    public static ColorSwatch FindAllColorVariations(IEnumerable<SKColor> colors, bool ignoreLimits = false)
    {
        SKColor bestVibrantColor = SKColor.Empty;
        SKColor bestLightVibrantColor = SKColor.Empty;
        SKColor bestDarkVibrantColor = SKColor.Empty;
        SKColor bestMutedColor = SKColor.Empty;
        SKColor bestLightMutedColor = SKColor.Empty;
        SKColor bestDarkMutedColor = SKColor.Empty;
        float bestVibrantScore = float.MinValue;
        float bestLightVibrantScore = float.MinValue;
        float bestDarkVibrantScore = float.MinValue;
        float bestMutedScore = float.MinValue;
        float bestLightMutedScore = float.MinValue;
        float bestDarkMutedScore = float.MinValue;

        //ugly but at least we only loop through the enumerable once ¯\_(ツ)_/¯
        foreach (SKColor color in colors)
        {
            static void SetIfBetterScore(ref float bestScore, ref SKColor bestColor, SKColor newColor, ColorType type, bool ignoreLimits)
            {
                float newScore = GetScore(newColor, type, ignoreLimits);
                if (newScore > bestScore)
                {
                    bestScore = newScore;
                    bestColor = newColor;
                }
            }

            SetIfBetterScore(ref bestVibrantScore, ref bestVibrantColor, color, ColorType.Vibrant, ignoreLimits);
            SetIfBetterScore(ref bestLightVibrantScore, ref bestLightVibrantColor, color, ColorType.LightVibrant, ignoreLimits);
            SetIfBetterScore(ref bestDarkVibrantScore, ref bestDarkVibrantColor, color, ColorType.DarkVibrant, ignoreLimits);
            SetIfBetterScore(ref bestMutedScore, ref bestMutedColor, color, ColorType.Muted, ignoreLimits);
            SetIfBetterScore(ref bestLightMutedScore, ref bestLightMutedColor, color, ColorType.LightMuted, ignoreLimits);
            SetIfBetterScore(ref bestDarkMutedScore, ref bestDarkMutedColor, color, ColorType.DarkMuted, ignoreLimits);
        }

        return new ColorSwatch
        {
            Vibrant = bestVibrantColor,
            LightVibrant = bestLightVibrantColor,
            DarkVibrant = bestDarkVibrantColor,
            Muted = bestMutedColor,
            LightMuted = bestLightMutedColor,
            DarkMuted = bestDarkMutedColor,
        };
    }

    private static float GetScore(SKColor color, ColorType type, bool ignoreLimits = false)
    {
        static float InvertDiff(float value, float target)
        {
            return 1 - Math.Abs(value - target);
        }

        color.ToHsl(out float _, out float saturation, out float luma);
        saturation /= 100f;
        luma /= 100f;

        if (!ignoreLimits && ((saturation <= GetMinSaturation(type)) || (saturation >= GetMaxSaturation(type)) || (luma <= GetMinLuma(type)) || (luma >= GetMaxLuma(type))))
        {
            //if either saturation or luma falls outside the min-max, return the
            //lowest score possible unless we're ignoring these limits.
            return float.MinValue;
        }

        float totalValue = (InvertDiff(saturation, GetTargetSaturation(type)) * WEIGHT_SATURATION) + (InvertDiff(luma, GetTargetLuma(type)) * WEIGHT_LUMA);

        const float TOTAL_WEIGHT = WEIGHT_SATURATION + WEIGHT_LUMA;

        return totalValue / TOTAL_WEIGHT;
    }

    #region Constants

    private const float TARGET_DARK_LUMA = 0.26f;
    private const float MAX_DARK_LUMA = 0.45f;
    private const float MIN_LIGHT_LUMA = 0.55f;
    private const float TARGET_LIGHT_LUMA = 0.74f;
    private const float MIN_NORMAL_LUMA = 0.3f;
    private const float TARGET_NORMAL_LUMA = 0.5f;
    private const float MAX_NORMAL_LUMA = 0.7f;
    private const float TARGET_MUTES_SATURATION = 0.3f;
    private const float MAX_MUTES_SATURATION = 0.3f;
    private const float TARGET_VIBRANT_SATURATION = 1.0f;
    private const float MIN_VIBRANT_SATURATION = 0.35f;
    private const float WEIGHT_SATURATION = 3f;
    private const float WEIGHT_LUMA = 5f;

    private static float GetTargetLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TARGET_NORMAL_LUMA,
        ColorType.LightVibrant => TARGET_LIGHT_LUMA,
        ColorType.DarkVibrant => TARGET_DARK_LUMA,
        ColorType.Muted => TARGET_NORMAL_LUMA,
        ColorType.LightMuted => TARGET_LIGHT_LUMA,
        ColorType.DarkMuted => TARGET_DARK_LUMA,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MIN_NORMAL_LUMA,
        ColorType.LightVibrant => MIN_LIGHT_LUMA,
        ColorType.DarkVibrant => 0f,
        ColorType.Muted => MIN_NORMAL_LUMA,
        ColorType.LightMuted => MIN_LIGHT_LUMA,
        ColorType.DarkMuted => 0,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMaxLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MAX_NORMAL_LUMA,
        ColorType.LightVibrant => 1f,
        ColorType.DarkVibrant => MAX_DARK_LUMA,
        ColorType.Muted => MAX_NORMAL_LUMA,
        ColorType.LightMuted => 1f,
        ColorType.DarkMuted => MAX_DARK_LUMA,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetTargetSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TARGET_VIBRANT_SATURATION,
        ColorType.LightVibrant => TARGET_VIBRANT_SATURATION,
        ColorType.DarkVibrant => TARGET_VIBRANT_SATURATION,
        ColorType.Muted => TARGET_MUTES_SATURATION,
        ColorType.LightMuted => TARGET_MUTES_SATURATION,
        ColorType.DarkMuted => TARGET_MUTES_SATURATION,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MIN_VIBRANT_SATURATION,
        ColorType.LightVibrant => MIN_VIBRANT_SATURATION,
        ColorType.DarkVibrant => MIN_VIBRANT_SATURATION,
        ColorType.Muted => 0,
        ColorType.LightMuted => 0,
        ColorType.DarkMuted => 0,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMaxSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => 1f,
        ColorType.LightVibrant => 1f,
        ColorType.DarkVibrant => 1f,
        ColorType.Muted => MAX_MUTES_SATURATION,
        ColorType.LightMuted => MAX_MUTES_SATURATION,
        ColorType.DarkMuted => MAX_MUTES_SATURATION,
        _ => throw new ArgumentException(nameof(colorType))
    };

    #endregion
}

internal readonly struct ColorRanges
{
    public readonly byte RedRange;
    public readonly byte GreenRange;
    public readonly byte BlueRange;

    public ColorRanges(byte redRange, byte greenRange, byte blueRange)
    {
        this.RedRange = redRange;
        this.GreenRange = greenRange;
        this.BlueRange = blueRange;
    }
}

internal class ColorCube
{
    private readonly int _from;
    private readonly int _length;
    private SortTarget _currentOrder = SortTarget.None;

    public ColorCube(in Span<SKColor> fullColorList, int from, int length, SortTarget preOrdered)
    {
        this._from = from;
        this._length = length;

        OrderColors(fullColorList.Slice(from, length), preOrdered);
    }

    private void OrderColors(in Span<SKColor> colors, SortTarget preOrdered)
    {
        if (colors.Length < 2) return;
        ColorRanges colorRanges = GetColorRanges(colors);

        if ((colorRanges.RedRange > colorRanges.GreenRange) && (colorRanges.RedRange > colorRanges.BlueRange))
        {
            if (preOrdered != SortTarget.Red)
                RadixLikeSortRed.Sort(colors);

            _currentOrder = SortTarget.Red;
        }
        else if (colorRanges.GreenRange > colorRanges.BlueRange)
        {
            if (preOrdered != SortTarget.Green)
                RadixLikeSortGreen.Sort(colors);

            _currentOrder = SortTarget.Green;
        }
        else
        {
            if (preOrdered != SortTarget.Blue)
                RadixLikeSortBlue.Sort(colors);

            _currentOrder = SortTarget.Blue;
        }
    }

    private ColorRanges GetColorRanges(in Span<SKColor> colors)
    {
        if (colors.Length < 512)
        {
            byte redMin = byte.MaxValue;
            byte redMax = byte.MinValue;
            byte greenMin = byte.MaxValue;
            byte greenMax = byte.MinValue;
            byte blueMin = byte.MaxValue;
            byte blueMax = byte.MinValue;

            for (int i = 0; i < colors.Length; i++)
            {
                SKColor color = colors[i];
                if (color.Red < redMin) redMin = color.Red;
                if (color.Red > redMax) redMax = color.Red;
                if (color.Green < greenMin) greenMin = color.Green;
                if (color.Green > greenMax) greenMax = color.Green;
                if (color.Blue < blueMin) blueMin = color.Blue;
                if (color.Blue > blueMax) blueMax = color.Blue;
            }

            return new ColorRanges((byte)(redMax - redMin), (byte)(greenMax - greenMin), (byte)(blueMax - blueMin));
        }
        else
        {
            Span<bool> redBuckets = stackalloc bool[256];
            Span<bool> greenBuckets = stackalloc bool[256];
            Span<bool> blueBuckets = stackalloc bool[256];

            for (int i = 0; i < colors.Length; i++)
            {
                SKColor color = colors[i];
                redBuckets[color.Red] = true;
                greenBuckets[color.Green] = true;
                blueBuckets[color.Blue] = true;
            }

            byte redMin = 0;
            byte redMax = 0;
            byte greenMin = 0;
            byte greenMax = 0;
            byte blueMin = 0;
            byte blueMax = 0;

            for (byte i = 0; i < redBuckets.Length; i++)
                if (redBuckets[i])
                {
                    redMin = i;
                    break;
                }

            for (int i = redBuckets.Length - 1; i >= 0; i--)
                if (redBuckets[i])
                {
                    redMax = (byte)i;
                    break;
                }

            for (byte i = 0; i < greenBuckets.Length; i++)
                if (greenBuckets[i])
                {
                    greenMin = i;
                    break;
                }

            for (int i = greenBuckets.Length - 1; i >= 0; i--)
                if (greenBuckets[i])
                {
                    greenMax = (byte)i;
                    break;
                }

            for (byte i = 0; i < blueBuckets.Length; i++)
                if (blueBuckets[i])
                {
                    blueMin = i;
                    break;
                }

            for (int i = blueBuckets.Length - 1; i >= 0; i--)
                if (blueBuckets[i])
                {
                    blueMax = (byte)i;
                    break;
                }

            return new ColorRanges((byte)(redMax - redMin), (byte)(greenMax - greenMin), (byte)(blueMax - blueMin));
        }
    }

    internal bool TrySplit(in Span<SKColor> fullColorList, [NotNullWhen(returnValue: true)] out ColorCube? a, [NotNullWhen(returnValue: true)] out ColorCube? b)
    {
        Span<SKColor> colors = fullColorList.Slice(_from, _length);

        if (colors.Length < 2)
        {
            a = null;
            b = null;
            return false;
        }

        int median = colors.Length / 2;

        a = new ColorCube(fullColorList, _from, median, _currentOrder);
        b = new ColorCube(fullColorList, _from + median, colors.Length - median, _currentOrder);

        return true;
    }

    internal SKColor GetAverageColor(in Span<SKColor> fullColorList)
    {
        Span<SKColor> colors = fullColorList.Slice(_from, _length);

        int r = 0, g = 0, b = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            SKColor color = colors[i];
            r += color.Red;
            g += color.Green;
            b += color.Blue;
        }

        return new SKColor(
            (byte)(r / colors.Length),
            (byte)(g / colors.Length),
            (byte)(b / colors.Length)
        );
    }
}

internal static class RadixLikeSortRed
{
    #region Methods

    public static void Sort(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
            counts[span[i].Red]++;

        Span<SKColor[]> buckets = ArrayPool<SKColor[]>.Shared.Rent(256).AsSpan(0, 256);
        for (int i = 0; i < counts.Length; i++)
            buckets[i] = ArrayPool<SKColor>.Shared.Rent(counts[i]);

        Span<int> currentBucketIndex = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
        {
            SKColor color = span[i];
            int index = color.Red;
            SKColor[] bucket = buckets[index];
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            bucket[bucketIndex] = color;
        }

        int newIndex = 0;
        for (int i = 0; i < buckets.Length; i++)
        {
            Span<SKColor> bucket = buckets[i].AsSpan(0, counts[i]);
            for (int j = 0; j < bucket.Length; j++)
                span[newIndex++] = bucket[j];

            ArrayPool<SKColor>.Shared.Return(buckets[i]);
        }
    }

    #endregion
}

internal static class RadixLikeSortGreen
{
    #region Methods

    public static void Sort(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
            counts[span[i].Green]++;

        Span<SKColor[]> buckets = ArrayPool<SKColor[]>.Shared.Rent(256).AsSpan(0, 256);
        for (int i = 0; i < counts.Length; i++)
            buckets[i] = ArrayPool<SKColor>.Shared.Rent(counts[i]);

        Span<int> currentBucketIndex = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
        {
            SKColor color = span[i];
            int index = color.Green;
            SKColor[] bucket = buckets[index];
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            bucket[bucketIndex] = color;
        }

        int newIndex = 0;
        for (int i = 0; i < buckets.Length; i++)
        {
            Span<SKColor> bucket = buckets[i].AsSpan(0, counts[i]);
            for (int j = 0; j < bucket.Length; j++)
                span[newIndex++] = bucket[j];

            ArrayPool<SKColor>.Shared.Return(buckets[i]);
        }
    }

    #endregion
}

internal static class RadixLikeSortBlue
{
    #region Methods

    public static void Sort(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
            counts[span[i].Blue]++;

        Span<SKColor[]> buckets = ArrayPool<SKColor[]>.Shared.Rent(256).AsSpan(0, 256);
        for (int i = 0; i < counts.Length; i++)
            buckets[i] = ArrayPool<SKColor>.Shared.Rent(counts[i]);

        Span<int> currentBucketIndex = stackalloc int[256];
        for (int i = 0; i < span.Length; i++)
        {
            SKColor color = span[i];
            int index = color.Blue;
            SKColor[] bucket = buckets[index];
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            bucket[bucketIndex] = color;
        }

        int newIndex = 0;
        for (int i = 0; i < buckets.Length; i++)
        {
            Span<SKColor> bucket = buckets[i].AsSpan(0, counts[i]);
            for (int j = 0; j < bucket.Length; j++)
                span[newIndex++] = bucket[j];

            ArrayPool<SKColor>.Shared.Return(buckets[i]);
        }
    }

    #endregion
}

public enum SortTarget
{
    None, Red, Green, Blue
}

#endregion