using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace Artemis.UI.Extensions;

public class BitmapExtensions
{
    public static Bitmap LoadAndResize(string file, int size, bool fit)
    {
        using SKBitmap source = SKBitmap.Decode(file);
        return Resize(source, size, fit);
    }

    public static Bitmap LoadAndResize(Stream stream, int size, bool fit)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using MemoryStream copy = new();
        stream.CopyTo(copy);
        copy.Seek(0, SeekOrigin.Begin);
        using SKBitmap source = SKBitmap.Decode(copy);
        return Resize(source, size, fit);
    }

    private static Bitmap Resize(SKBitmap source, int size, bool fit)
    {
        if (!fit)
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

            // Encode via SKImage for compatibility
            using SKImage image = SKImage.FromBitmap(resizedBitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            return new Bitmap(data.AsStream());
        }
        else
        {
            // Fit the image inside a size x size square without cropping.
            // Compute scale based on the larger dimension.
            float scale = (float)size / Math.Max(source.Width, source.Height);
            int targetW = Math.Max(1, (int)Math.Floor(source.Width * scale));
            int targetH = Math.Max(1, (int)Math.Floor(source.Height * scale));

            // Resize maintaining aspect ratio.
            using SKBitmap resizedAspect = source.Resize(new SKImageInfo(targetW, targetH), SKFilterQuality.High);

            // Create final square canvas and draw the fitted image centered.
            using SKBitmap finalBitmap = new(size, size);
            using (SKCanvas canvas = new(finalBitmap))
            {
                // Clear to transparent.
                canvas.Clear(SKColors.Transparent);

                int offsetX = (size - targetW) / 2;
                int offsetY = (size - targetH) / 2;
                canvas.DrawBitmap(resizedAspect, new SKPoint(offsetX, offsetY));
                canvas.Flush();
            }

            using SKImage image = SKImage.FromBitmap(finalBitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            return new Bitmap(data.AsStream());
        }
    }
}