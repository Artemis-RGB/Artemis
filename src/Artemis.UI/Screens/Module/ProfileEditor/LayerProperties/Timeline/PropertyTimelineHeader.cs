using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTimelineHeader : FrameworkElement
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(PropertyTimelineHeader),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(PropertyTimelineHeader),
            new FrameworkPropertyMetadata(TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PixelsPerSecondProperty = DependencyProperty.Register(nameof(PixelsPerSecond), typeof(int), typeof(PropertyTimelineHeader),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsRender));


        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(PropertyTimelineHeader),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty VisibleWidthProperty = DependencyProperty.Register(nameof(VisibleWidth), typeof(double), typeof(PropertyTimelineHeader),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

        private double _subd1;
        private double _subd2;
        private double _subd3;

        public Brush Fill
        {
            get => (Brush) GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public FontFamily FontFamily
        {
            get => (FontFamily) GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public int PixelsPerSecond
        {
            get => (int) GetValue(PixelsPerSecondProperty);
            set => SetValue(PixelsPerSecondProperty, value);
        }

        public double HorizontalOffset
        {
            get => (double) GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public double VisibleWidth
        {
            get => (double) GetValue(VisibleWidthProperty);
            set => SetValue(VisibleWidthProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            UpdateTimeScale();

            var linePen = new Pen(Fill, 1);
            var width = HorizontalOffset + VisibleWidth;
            var frameStart = 0;

            var units = PixelsPerSecond / _subd1;
            var offsetUnits = frameStart * PixelsPerSecond % units;

            // Labels
            var count = (width + offsetUnits) / units;
            for (var i = 0; i < count; i++)
            {
                var x = i * units - offsetUnits + 1;
                // Add a 100px margin to allow the text to partially render when needed
                if (x < HorizontalOffset - 100 || x > HorizontalOffset + width)
                    continue;

                var t = TimeSpan.FromSeconds((i * units - offsetUnits) / PixelsPerSecond + frameStart);
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
            for (var i = 0; i < count; i++)
            {
                var x = i * units - offsetUnits + 1;
                if (x > HorizontalOffset && x < HorizontalOffset + width)
                    drawingContext.DrawLine(linePen, new Point(x, 20), new Point(x, 30));
            }

            // Small ticks
            var mul = _subd3 / _subd2;
            units = PixelsPerSecond / _subd3;
            count = (width + offsetUnits) / units;
            for (var i = 0; i < count; i++)
            {
                if (Math.Abs(i % mul) < 0.001) continue;
                var x = i * units - offsetUnits + 1;
                if (x > HorizontalOffset && x < HorizontalOffset + width)
                    drawingContext.DrawLine(linePen, new Point(x, 25), new Point(x, 30));
            }
        }

        private void RenderLabel(DrawingContext drawingContext, string text, double x)
        {
            var typeFace = new Typeface(FontFamily, new FontStyle(), new FontWeight(), new FontStretch());
            var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeFace, 9, Fill, null, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            if (x == 1)
                drawingContext.DrawText(formattedText, new Point(x, 2));
            else
                drawingContext.DrawText(formattedText, new Point(x - formattedText.Width / 2, 2));
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
            else if (PixelsPerSecond > 100)
                subds = new object[] {4d, 4d, 8d};
            else if (PixelsPerSecond > 90)
                subds = new object[] {4d, 4d, 8d};
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
            {
                // 1s per pixel
                subds = new object[] {1d / 60d, 1d / 60d, 1d / 15d};
            }

            _subd1 = (double) subds[0]; // big ticks / labels
            _subd2 = (double) subds[1]; // medium ticks
            _subd3 = (double) subds[2]; // small ticks
        }
    }
}