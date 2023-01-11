using SkiaSharp;
using System;
using System.Buffers;

namespace Artemis.Core.ColorScience;

//HACK DarthAffe 17.11.2022: Sorting is a really hot path in the quantizer, therefore abstracting this into cleaner code (one method with parameter or something like that) sadly has a well measurable performance impact.
internal static class QuantizerSort
{
    #region Methods

    public static void SortRed(in Span<SKColor> colors)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in colors)
            counts[t.Red]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(colors.Length);

        try
        {
            Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, colors.Length);
            Span<int> currentBucketIndex = stackalloc int[256];

            int offset = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                currentBucketIndex[i] = offset;
                offset += counts[i];
            }

            foreach (SKColor color in colors)
            {
                int index = color.Red;
                int bucketIndex = currentBucketIndex[index];
                currentBucketIndex[index]++;
                buckets[bucketIndex] = color;
            }

            buckets.CopyTo(colors);
        }
        finally
        {
            ArrayPool<SKColor>.Shared.Return(bucketsArray);
        }
    }

    public static void SortGreen(in Span<SKColor> colors)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in colors)
            counts[t.Green]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(colors.Length);

        try
        {
            Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, colors.Length);
            Span<int> currentBucketIndex = stackalloc int[256];

            int offset = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                currentBucketIndex[i] = offset;
                offset += counts[i];
            }

            foreach (SKColor color in colors)
            {
                int index = color.Green;
                int bucketIndex = currentBucketIndex[index];
                currentBucketIndex[index]++;
                buckets[bucketIndex] = color;
            }

            buckets.CopyTo(colors);
        }
        finally
        {
            ArrayPool<SKColor>.Shared.Return(bucketsArray);
        }
    }

    public static void SortBlue(in Span<SKColor> colors)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in colors)
            counts[t.Blue]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(colors.Length);

        try
        {
            Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, colors.Length);
            Span<int> currentBucketIndex = stackalloc int[256];

            int offset = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                currentBucketIndex[i] = offset;
                offset += counts[i];
            }

            foreach (SKColor color in colors)
            {
                int index = color.Blue;
                int bucketIndex = currentBucketIndex[index];
                currentBucketIndex[index]++;
                buckets[bucketIndex] = color;
            }

            buckets.CopyTo(colors);
        }
        finally
        {
            ArrayPool<SKColor>.Shared.Return(bucketsArray);
        }
    }

    #endregion
}
