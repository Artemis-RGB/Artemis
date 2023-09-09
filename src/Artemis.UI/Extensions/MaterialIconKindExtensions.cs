using System;
using System.IO;
using Material.Icons;
using SkiaSharp;

namespace Artemis.UI.Extensions;

public static class MaterialIconKindExtensions
{
    public static Stream EncodeToBitmap(this MaterialIconKind icon, int size, int margin, SKColor color)
    {
        string geometrySource = MaterialIconDataProvider.GetData(icon);

        SKBitmap bitmap = new(size, size);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Transparent);

            // Parse and render the geometry data using SkiaSharp's SKPath
            using SKPath path = SKPath.ParseSvgPathData(geometrySource);
            using SKPaint paint = new() {Color = color, IsAntialias = true,};

            // Calculate scaling and translation to fit the icon in the 100x100 area with 14 pixels margin
            float scale = Math.Min(size / path.Bounds.Width, size / path.Bounds.Height);
            path.Transform(SKMatrix.CreateTranslation(path.Bounds.Left * -1, path.Bounds.Top * -1));
            path.Transform(SKMatrix.CreateScale(scale, scale));
            canvas.Scale((size - margin * 2) / (float) size, (size - margin * 2) / (float) size, size / 2f, size / 2f);
            canvas.DrawPath(path, paint);
        }

        MemoryStream stream = new();
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        return stream;
    }
}