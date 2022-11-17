using SkiaSharp;
using System;
using System.Buffers;

namespace Artemis.Core.ColorScience;

internal static class QuantizerSort
{
    #region Methods

    public static void SortRed(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in span)
            counts[t.Red]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(span.Length);
        Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, span.Length);
        Span<int> currentBucketIndex = stackalloc int[256];

        int offset = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            currentBucketIndex[i] = offset;
            offset += counts[i];
        }

        foreach (SKColor color in span)
        {
            int index = color.Red;
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            buckets[bucketIndex] = color;
        }

        buckets.CopyTo(span);

        ArrayPool<SKColor>.Shared.Return(bucketsArray);
    }

    public static void SortGreen(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in span)
            counts[t.Green]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(span.Length);
        Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, span.Length);
        Span<int> currentBucketIndex = stackalloc int[256];

        int offset = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            currentBucketIndex[i] = offset;
            offset += counts[i];
        }

        foreach (SKColor color in span)
        {
            int index = color.Green;
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            buckets[bucketIndex] = color;
        }

        buckets.CopyTo(span);

        ArrayPool<SKColor>.Shared.Return(bucketsArray);
    }

    public static void SortBlue(in Span<SKColor> span)
    {
        Span<int> counts = stackalloc int[256];
        foreach (SKColor t in span)
            counts[t.Blue]++;

        SKColor[] bucketsArray = ArrayPool<SKColor>.Shared.Rent(span.Length);
        Span<SKColor> buckets = bucketsArray.AsSpan().Slice(0, span.Length);
        Span<int> currentBucketIndex = stackalloc int[256];

        int offset = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            currentBucketIndex[i] = offset;
            offset += counts[i];
        }

        foreach (SKColor color in span)
        {
            int index = color.Blue;
            int bucketIndex = currentBucketIndex[index];
            currentBucketIndex[index]++;
            buckets[bucketIndex] = color;
        }

        buckets.CopyTo(span);

        ArrayPool<SKColor>.Shared.Return(bucketsArray);
    }

    #endregion
}
