using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Artemis.Core.ColorScience;

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
    private const int BYTES_PER_COLOR = 4;
    private static readonly int ELEMENTS_PER_VECTOR = Vector<byte>.Count / BYTES_PER_COLOR;
    private static readonly int BYTES_PER_VECTOR = ELEMENTS_PER_VECTOR * BYTES_PER_COLOR;

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

    private unsafe ColorRanges GetColorRanges(in ReadOnlySpan<SKColor> colors)
    {
        if (Vector.IsHardwareAccelerated && (colors.Length >= Vector<byte>.Count))
        {
            int chunks = colors.Length / ELEMENTS_PER_VECTOR;
            int missingElements = colors.Length - (chunks * ELEMENTS_PER_VECTOR);

            Vector<byte> max = Vector<byte>.Zero;
            Vector<byte> min = new(byte.MaxValue);

            ReadOnlySpan<byte> colorBytes = MemoryMarshal.AsBytes(colors);
            fixed (byte* colorPtr = &MemoryMarshal.GetReference(colorBytes))
            {
                byte* current = colorPtr;
                for (int i = 0; i < chunks; i++)
                {
                    Vector<byte> currentVector = *(Vector<byte>*)current;

                    max = Vector.Max(max, currentVector);
                    min = Vector.Min(min, currentVector);

                    current += BYTES_PER_VECTOR;
                }
            }

            byte redMin = byte.MaxValue;
            byte redMax = byte.MinValue;
            byte greenMin = byte.MaxValue;
            byte greenMax = byte.MinValue;
            byte blueMin = byte.MaxValue;
            byte blueMax = byte.MinValue;

            for (int i = 0; i < BYTES_PER_VECTOR; i += BYTES_PER_COLOR)
            {
                if (min[i + 2] < redMin) redMin = min[i + 2];
                if (max[i + 2] > redMax) redMax = max[i + 2];
                if (min[i + 1] < greenMin) greenMin = min[i + 1];
                if (max[i + 1] > greenMax) greenMax = max[i + 1];
                if (min[i] < blueMin) blueMin = min[i];
                if (max[i] > blueMax) blueMax = max[i];
            }

            for (int i = 0; i < missingElements; i++)
            {
                SKColor color = colors[^(i + 1)];

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
            byte redMin = byte.MaxValue;
            byte redMax = byte.MinValue;
            byte greenMin = byte.MaxValue;
            byte greenMax = byte.MinValue;
            byte blueMin = byte.MaxValue;
            byte blueMax = byte.MinValue;

            foreach (SKColor color in colors)
            {
                if (color.Red < redMin) redMin = color.Red;
                if (color.Red > redMax) redMax = color.Red;
                if (color.Green < greenMin) greenMin = color.Green;
                if (color.Green > greenMax) greenMax = color.Green;
                if (color.Blue < blueMin) blueMin = color.Blue;
                if (color.Blue > blueMax) blueMax = color.Blue;
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

    internal SKColor GetAverageColor(in ReadOnlySpan<SKColor> fullColorList)
    {
        ReadOnlySpan<SKColor> colors = fullColorList.Slice(_from, _length);

        int r = 0, g = 0, b = 0;
        foreach (SKColor color in colors)
        {
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
