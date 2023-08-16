using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace Artemis.UI.Extensions;

public class BitmapExtensions
{
    public static Bitmap LoadAndResize(string file, int size)
    {
        using SKBitmap source = SKBitmap.Decode(file);
        return Resize(source, size);
    }

    public static Bitmap LoadAndResize(Stream stream, int size)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using MemoryStream copy = new();
        stream.CopyTo(copy);
        copy.Seek(0, SeekOrigin.Begin);
        using SKBitmap source = SKBitmap.Decode(copy);
        return Resize(source, size);
    }

    private static Bitmap Resize(SKBitmap source, int size)
    {
        // Get smaller dimension.
        int minDim = Math.Min(source.Width, source.Height);

        // Calculate crop rectangle position for center crop.
        int deltaX = (source.Width - minDim) / 2;
        int deltaY = (source.Height - minDim) / 2;

        // Create crop rectangle.
        SKRectI rect = new(deltaX, deltaY, deltaX + minDim, deltaY + minDim);

        // Do the actual cropping of the bitmap.
        using SKBitmap croppedBitmap = new(minDim, minDim);
        source.ExtractSubset(croppedBitmap, rect);

        // Resize to the desired size after cropping.
        using SKBitmap resizedBitmap = croppedBitmap.Resize(new SKImageInfo(size, size), SKFilterQuality.High);

        return new Bitmap(resizedBitmap.Encode(SKEncodedImageFormat.Png, 100).AsStream());
    }
}