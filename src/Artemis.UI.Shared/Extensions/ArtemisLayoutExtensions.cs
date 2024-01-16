using System;
using System.IO;
using Artemis.Core;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RGB.NET.Core;
using Color = Avalonia.Media.Color;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="ArtemisLayout" /> type.
/// </summary>
public static class ArtemisLayoutExtensions
{
    /// <summary>
    /// Renders the layout to a bitmap.
    /// </summary>
    /// <param name="layout">The layout to render</param>
    /// <returns>The resulting bitmap.</returns>
    public static RenderTargetBitmap RenderLayout(this ArtemisLayout layout, bool previewLeds)
    {
        string? path = layout.Image?.LocalPath;

        // Create a bitmap that'll be used to render the device and LED images just once
        // Render 4 times the actual size of the device to make sure things look sharp when zoomed in
        RenderTargetBitmap renderTargetBitmap = new(new PixelSize((int) layout.RgbLayout.Width * 2, (int) layout.RgbLayout.Height * 2));

        using DrawingContext context = renderTargetBitmap.CreateDrawingContext();

        // Draw device background
        if (path != null && File.Exists(path))
        {
            using Bitmap bitmap = new(path);
            using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(renderTargetBitmap.PixelSize);
            context.DrawImage(scaledBitmap, new Rect(scaledBitmap.Size));
        }

        // Draw LED images
        foreach (ArtemisLedLayout led in layout.Leds)
        {
            string? ledPath = led.Image?.LocalPath;
            if (ledPath == null || !File.Exists(ledPath))
                continue;
            using Bitmap bitmap = new(ledPath);
            using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(new PixelSize((led.RgbLayout.Width * 2).RoundToInt(), (led.RgbLayout.Height * 2).RoundToInt()));
            context.DrawImage(scaledBitmap, new Rect(led.RgbLayout.X * 2, led.RgbLayout.Y * 2, scaledBitmap.Size.Width, scaledBitmap.Size.Height));
        }

        if (!previewLeds)
            return renderTargetBitmap;

        // Draw LED geometry using a rainbow gradient
        ColorGradient colors = ColorGradient.GetUnicornBarf();
        colors.ToggleSeamless();
        context.PushTransform(Matrix.CreateScale(2, 2));
        foreach (ArtemisLedLayout led in layout.Leds)
        {
            Geometry? geometry = CreateLedGeometry(led);
            if (geometry == null)
                continue;

            Color color = colors.GetColor((led.RgbLayout.X + led.RgbLayout.Width / 2) / layout.RgbLayout.Width).ToColor();
            SolidColorBrush fillBrush = new() {Color = color, Opacity = 0.4};
            SolidColorBrush penBrush = new() {Color = color};
            Pen pen = new(penBrush) {LineJoin = PenLineJoin.Round};
            context.DrawGeometry(fillBrush, pen, geometry);
        }

        return renderTargetBitmap;
    }

    private static Geometry? CreateLedGeometry(ArtemisLedLayout led)
    {
        // The minimum required size for geometry to be created
        if (led.RgbLayout.Width < 2 || led.RgbLayout.Height < 2)
            return null;

        switch (led.RgbLayout.Shape)
        {
            case Shape.Custom:
                if (led.DeviceLayout.RgbLayout.Type is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
                    return CreateCustomGeometry(led, 2.0);
                return CreateCustomGeometry(led, 1.0);
            case Shape.Rectangle:
                if (led.DeviceLayout.RgbLayout.Type is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
                    return CreateKeyCapGeometry(led);
                return CreateRectangleGeometry(led);
            case Shape.Circle:
                return CreateCircleGeometry(led);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static RectangleGeometry CreateRectangleGeometry(ArtemisLedLayout led)
    {
        return new RectangleGeometry(new Rect(led.RgbLayout.X + 0.5, led.RgbLayout.Y + 0.5, led.RgbLayout.Width - 1, led.RgbLayout.Height - 1));
    }

    private static EllipseGeometry CreateCircleGeometry(ArtemisLedLayout led)
    {
        return new EllipseGeometry(new Rect(led.RgbLayout.X + 0.5, led.RgbLayout.Y + 0.5, led.RgbLayout.Width - 1, led.RgbLayout.Height - 1));
    }

    private static RectangleGeometry CreateKeyCapGeometry(ArtemisLedLayout led)
    {
        return new RectangleGeometry(new Rect(led.RgbLayout.X + 1, led.RgbLayout.Y + 1, led.RgbLayout.Width - 2, led.RgbLayout.Height - 2));
    }

    private static Geometry? CreateCustomGeometry(ArtemisLedLayout led, double deflateAmount)
    {
        try
        {
            if (led.RgbLayout.ShapeData == null)
                return null;

            double width = led.RgbLayout.Width - deflateAmount;
            double height = led.RgbLayout.Height - deflateAmount;

            Geometry geometry = Geometry.Parse(led.RgbLayout.ShapeData);
            geometry.Transform = new TransformGroup
            {
                Children = new Transforms
                {
                    new ScaleTransform(width, height),
                    new TranslateTransform(led.RgbLayout.X + deflateAmount / 2, led.RgbLayout.Y + deflateAmount / 2)
                }
            };
            return geometry;
        }
        catch (Exception)
        {
            return CreateRectangleGeometry(led);
        }
    }
}