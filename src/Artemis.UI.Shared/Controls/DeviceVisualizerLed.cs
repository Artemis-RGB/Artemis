using System;
using System.IO;
using Artemis.Core;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RGB.NET.Core;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;

namespace Artemis.UI.Shared;

internal class DeviceVisualizerLed
{
    private readonly SolidColorBrush _fillBrush;
    private readonly Pen _pen;
    private readonly SolidColorBrush _penBrush;

    public DeviceVisualizerLed(ArtemisLed led)
    {
        Led = led;
        LedRect = new Rect(
            Led.RgbLed.Location.X,
            Led.RgbLed.Location.Y,
            Led.RgbLed.Size.Width,
            Led.RgbLed.Size.Height
        );

        _fillBrush = new SolidColorBrush();
        _penBrush = new SolidColorBrush();
        _pen = new Pen(_penBrush) {LineJoin = PenLineJoin.Round};

        CreateLedGeometry();
    }

    public ArtemisLed Led { get; }
    public Rect LedRect { get; set; }
    public Geometry? DisplayGeometry { get; private set; }

    public void DrawBitmap(DrawingContext drawingContext, double scale)
    {
        if (Led.Layout?.Image == null || !File.Exists(Led.Layout.Image.LocalPath))
            return;

        try
        {
            using Bitmap bitmap = new(Led.Layout.Image.LocalPath);
            drawingContext.DrawImage(
                bitmap,
                new Rect(bitmap.Size),
                new Rect(Led.RgbLed.Location.X * scale, Led.RgbLed.Location.Y * scale, Led.RgbLed.Size.Width * scale, Led.RgbLed.Size.Height * scale)
            );
        }
        catch
        {
            // ignored
        }
    }

    public void RenderGeometry(DrawingContext drawingContext, bool dimmed)
    {
        if (DisplayGeometry == null)
            return;

        byte r = Led.RgbLed.Color.GetR();
        byte g = Led.RgbLed.Color.GetG();
        byte b = Led.RgbLed.Color.GetB();

        if (dimmed)
        {
            _fillBrush.Color = new Color(50, r, g, b);
            _penBrush.Color = new Color(100, r, g, b);
        }
        else
        {
            _fillBrush.Color = new Color(100, r, g, b);
            _penBrush.Color = new Color(255, r, g, b);
        }

        // Render the LED geometry
        drawingContext.DrawGeometry(_fillBrush, _pen, DisplayGeometry);
    }

    public bool HitTest(Point position)
    {
        return DisplayGeometry != null && DisplayGeometry.FillContains(position);
    }

    private void CreateLedGeometry()
    {
        // The minimum required size for geometry to be created
        if (Led.RgbLed.Size.Width < 2 || Led.RgbLed.Size.Height < 2)
            return;

        switch (Led.RgbLed.Shape)
        {
            case Shape.Custom:
                if (Led.RgbLed.Device.DeviceInfo.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
                    CreateCustomGeometry(2.0);
                else
                    CreateCustomGeometry(1.0);
                break;
            case Shape.Rectangle:
                if (Led.RgbLed.Device.DeviceInfo.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
                    CreateKeyCapGeometry();
                else
                    CreateRectangleGeometry();
                break;
            case Shape.Circle:
                CreateCircleGeometry();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CreateRectangleGeometry()
    {
        DisplayGeometry = new RectangleGeometry(new Rect(Led.RgbLed.Location.X + 0.5, Led.RgbLed.Location.Y + 0.5, Led.RgbLed.Size.Width - 1, Led.RgbLed.Size.Height - 1));
    }

    private void CreateCircleGeometry()
    {
        DisplayGeometry = new EllipseGeometry(new Rect(Led.RgbLed.Location.X + 0.5, Led.RgbLed.Location.Y + 0.5, Led.RgbLed.Size.Width - 1, Led.RgbLed.Size.Height - 1));
    }

    private void CreateKeyCapGeometry()
    {
        DisplayGeometry = new RectangleGeometry(new Rect(Led.RgbLed.Location.X + 1, Led.RgbLed.Location.Y + 1, Led.RgbLed.Size.Width - 2, Led.RgbLed.Size.Height - 2));
    }

    private void CreateCustomGeometry(double deflateAmount)
    {
        try
        {
            if (Led.RgbLed.ShapeData == null)
                return;

            double width = Led.RgbLed.Size.Width - deflateAmount;
            double height = Led.RgbLed.Size.Height - deflateAmount;

            Geometry geometry = Geometry.Parse(Led.RgbLed.ShapeData);
            geometry.Transform = new TransformGroup
            {
                Children = new Transforms
                {
                    new ScaleTransform(width, height),
                    new TranslateTransform(Led.RgbLed.Location.X + deflateAmount / 2, Led.RgbLed.Location.Y + deflateAmount / 2)
                }
            };
            DisplayGeometry = geometry;
        }
        catch (Exception)
        {
            CreateRectangleGeometry();
        }
    }
}