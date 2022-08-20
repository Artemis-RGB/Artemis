using System;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Artemis.Core.Services;

internal class ColorCube
{
    #region Properties & Fields

    private readonly int _from;
    private readonly int _length;
    private SortTarget _currentOrder = SortTarget.None;

    #endregion

    #region Constructors

    public ColorCube(in Span<SKColor> fullColorList, int from, int length, SortTarget preOrdered)
    {
        this._from = from;
        this._length = length;

        OrderColors(fullColorList.Slice(from, length), preOrdered);
    }

    #endregion

    #region Methods

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
        if (colors.Length < 70)
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
        else
        {
            int elementsPerVector = Vector<byte>.Count / 3;
            int chunks = colors.Length / elementsPerVector;
            int missingElements = colors.Length - (chunks * elementsPerVector);

            Vector<byte> max = Vector<byte>.Zero;
            Vector<byte> min = new(byte.MaxValue);

            Span<byte> chunkData = stackalloc byte[Vector<byte>.Count];
            int dataIndex = 0;
            for (int i = 0; i < chunks; i++)
            {
                int chunkDataIndex = 0;
                for (int j = 0; j < elementsPerVector; j++)
                {
                    SKColor color = colors[dataIndex];
                    chunkData[chunkDataIndex] = color.Red;
                    ++chunkDataIndex;
                    chunkData[chunkDataIndex] = color.Green;
                    ++chunkDataIndex;
                    chunkData[chunkDataIndex] = color.Blue;
                    ++chunkDataIndex;
                    ++dataIndex;
                }

                Vector<byte> chunkVector = new(chunkData);
                max = Vector.Max(max, chunkVector);
                min = Vector.Min(min, chunkVector);
            }

            byte redMin = byte.MaxValue;
            byte redMax = byte.MinValue;
            byte greenMin = byte.MaxValue;
            byte greenMax = byte.MinValue;
            byte blueMin = byte.MaxValue;
            byte blueMax = byte.MinValue;

            int vectorEntries = elementsPerVector * 3;
            for (int i = 0; i < vectorEntries; i += 3)
            {
                if (min[i] < redMin) redMin = min[i];
                if (max[i] > redMax) redMax = max[i];
                if (min[i + 1] < greenMin) greenMin = min[i + 1];
                if (max[i + 1] > greenMax) greenMax = max[i + 1];
                if (min[i + 2] < blueMin) blueMin = min[i + 2];
                if (max[i + 2] > blueMax) blueMax = max[i + 2];
            }

            for (int i = 0; i < missingElements; i++)
            {
                SKColor color = colors[dataIndex];
                if (color.Red < redMin) redMin = color.Red;
                if (color.Red > redMax) redMax = color.Red;
                if (color.Green < greenMin) greenMin = color.Green;
                if (color.Green > greenMax) greenMax = color.Green;
                if (color.Blue < blueMin) blueMin = color.Blue;
                if (color.Blue > blueMax) blueMax = color.Blue;

                ++dataIndex;
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
        foreach (SKColor color in colors)
        {
            r += color.Red;
            g += color.Green;
            b += color.Blue;
        }

        return new SKColor((byte)(r / colors.Length),
                           (byte)(g / colors.Length),
                           (byte)(b / colors.Length));
    }

    #endregion
}