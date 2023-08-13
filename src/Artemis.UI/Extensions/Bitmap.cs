using System.IO;
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
        int newWidth, newHeight;
        float aspectRatio = (float) source.Width / source.Height;

        if (aspectRatio > 1)
        {
            newWidth = size;
            newHeight = (int) (size / aspectRatio);
        }
        else
        {
            newWidth = (int) (size * aspectRatio);
            newHeight = size;
        }

        using SKBitmap resizedBitmap = source.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        return new Bitmap(resizedBitmap.Encode(SKEncodedImageFormat.Png, 100).AsStream());
    }
}