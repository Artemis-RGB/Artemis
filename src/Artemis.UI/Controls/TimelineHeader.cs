using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Artemis.UI.Controls;

public class TimelineHeader : Control
{
    public static readonly StyledProperty<Brush> ForegroundProperty = AvaloniaProperty.Register<TimelineHeader, Brush>(nameof(Foreground), new SolidColorBrush(Colors.Black));
    public static readonly StyledProperty<Brush> BackgroundProperty = AvaloniaProperty.Register<TimelineHeader, Brush>(nameof(Background), new SolidColorBrush(Colors.Transparent));
    public static readonly StyledProperty<FontFamily> FontFamilyProperty = AvaloniaProperty.Register<TimelineHeader, FontFamily>(nameof(FontFamily), FontFamily.Default);
    public static readonly StyledProperty<int> PixelsPerSecondProperty = AvaloniaProperty.Register<TimelineHeader, int>(nameof(PixelsPerSecond));
    public static readonly StyledProperty<double> HorizontalOffsetProperty = AvaloniaProperty.Register<TimelineHeader, double>(nameof(HorizontalOffset));
    public static readonly StyledProperty<double> VisibleWidthProperty = AvaloniaProperty.Register<TimelineHeader, double>(nameof(VisibleWidth));
    public static readonly StyledProperty<bool> OffsetFirstValueProperty = AvaloniaProperty.Register<TimelineHeader, bool>(nameof(OffsetFirstValue));

    private double _subd1;
    private double _subd2;
    private double _subd3;

    /// <inheritdoc />
    static TimelineHeader()
    {
        AffectsRender<TimelineHeader>(
            ForegroundProperty,
            BackgroundProperty,
            FontFamilyProperty,
            PixelsPerSecondProperty,
            HorizontalOffsetProperty,
            VisibleWidthProperty,
            OffsetFirstValueProperty
        );
    }

    public Brush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public Brush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public int PixelsPerSecond
    {
        get => GetValue(PixelsPerSecondProperty);
        set => SetValue(PixelsPerSecondProperty, value);
    }

    public double HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public double VisibleWidth
    {
        get => GetValue(VisibleWidthProperty);
        set => SetValue(VisibleWidthProperty, value);
    }

    public bool OffsetFirstValue
    {
        get => GetValue(OffsetFirstValueProperty);
        set => SetValue(OffsetFirstValueProperty, value);
    }

    public override void Render(DrawingContext drawingContext)
    {
        UpdateTimeScale();

        drawingContext.DrawRectangle(Background, null, new Rect(0, 0, Bounds.Width, 30));

        Pen linePen = new(Foreground);
        double width = HorizontalOffset + VisibleWidth;
        int frameStart = 0;

        double units = PixelsPerSecond / _subd1;
        double offsetUnits = frameStart * PixelsPerSecond % units;

        // Labels
        double count = (width + offsetUnits) / units;
        for (int i = 0; i < count; i++)
        {
            double x = i * units - offsetUnits;
            // Add a 100px margin to allow the text to partially render when needed
            if (x < HorizontalOffset - 100 || x > HorizontalOffset + width)
                continue;

            TimeSpan t = TimeSpan.FromSeconds((i * units - offsetUnits) / PixelsPerSecond + frameStart);
            // 0.00 is always formatted as 0.00
            if (t == TimeSpan.Zero)
                RenderLabel(drawingContext, "0.00", x);
            else if (PixelsPerSecond > 200)
                RenderLabel(drawingContext, $"{Math.Floor(t.TotalSeconds):00}.{t.Milliseconds:000}", x);
            else if (PixelsPerSecond > 60)
                RenderLabel(drawingContext, $"{Math.Floor(t.TotalSeconds):00}.{t.Milliseconds:000}", x);
            else
                RenderLabel(drawingContext, $"{Math.Floor(t.TotalMinutes):0}:{t.Seconds:00}", x);
        }

        // Large ticks
        units = PixelsPerSecond / _subd2;
        count = (width + offsetUnits) / units;
        for (int i = 0; i < count; i++)
        {
            double x = i * units - offsetUnits;
            if (x == 0 && OffsetFirstValue)
                drawingContext.DrawLine(linePen, new Point(1, 20), new Point(1, 30));
            else if (x > HorizontalOffset && x < HorizontalOffset + width)
                drawingContext.DrawLine(linePen, new Point(x, 20), new Point(x, 30));
        }

        // Small ticks
        double mul = _subd3 / _subd2;
        units = PixelsPerSecond / _subd3;
        count = (width + offsetUnits) / units;
        for (int i = 0; i < count; i++)
        {
            if (Math.Abs(i % mul) < 0.001) continue;
            double x = i * units - offsetUnits;
            if (x > HorizontalOffset && x < HorizontalOffset + width)
                drawingContext.DrawLine(linePen, new Point(x, 25), new Point(x, 30));
        }
    }

    private void RenderLabel(DrawingContext drawingContext, string text, double x)
    {
        Typeface typeFace = new(FontFamily);
        FormattedText formattedText = new(text, typeFace, 9, TextAlignment.Left, TextWrapping.NoWrap, Bounds.Size);
        if (x == 0 && OffsetFirstValue)
            drawingContext.DrawText(Foreground, new Point(2, 5), formattedText);
        else
            drawingContext.DrawText(Foreground, new Point(x - formattedText.Bounds.Width / 2, 5), formattedText);
    }

    private void UpdateTimeScale()
    {
        object[] subds;
        if (PixelsPerSecond > 350)
            subds = new object[] {12d, 12d, 60d};
        else if (PixelsPerSecond > 250)
            subds = new object[] {6d, 12d, 60d};
        else if (PixelsPerSecond > 200)
            subds = new object[] {6d, 6d, 30d};
        else if (PixelsPerSecond > 150)
            subds = new object[] {4d, 4d, 20d};
        else if (PixelsPerSecond > 140)
            subds = new object[] {4d, 4d, 20d};
        else if (PixelsPerSecond > 90)
            subds = new object[] {2d, 4d, 20d};
        else if (PixelsPerSecond > 60)
            subds = new object[] {2d, 4d, 8d};
        else if (PixelsPerSecond > 40)
            subds = new object[] {1d, 2d, 10d};
        else if (PixelsPerSecond > 30)
            subds = new object[] {1d, 2d, 10d};
        else if (PixelsPerSecond > 10)
            subds = new object[] {1d / 2d, 1d / 2d, 1d / 2d};
        else if (PixelsPerSecond > 4)
            subds = new object[] {1d / 5d, 1d / 5d, 1d / 5d};
        else if (PixelsPerSecond > 3)
            subds = new object[] {1d / 10d, 1d / 10d, 1d / 5d};
        else if (PixelsPerSecond > 1)
            subds = new object[] {1d / 20d, 1d / 20d, 1d / 10d};
        else if (PixelsPerSecond >= 1)
            subds = new object[] {1d / 30d, 1d / 30d, 1d / 15d};
        else
            // 1s per pixel
            subds = new object[] {1d / 60d, 1d / 60d, 1d / 15d};

        _subd1 = (double) subds[0]; // big ticks / labels
        _subd2 = (double) subds[1]; // medium ticks
        _subd3 = (double) subds[2]; // small ticks
    }
}