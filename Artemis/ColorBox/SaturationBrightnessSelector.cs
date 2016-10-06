/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorBox
{
    public class SaturationBrightnessSelector : BaseSelector
    {
        public static readonly DependencyProperty OffsetPaddingProperty =
            DependencyProperty.Register("OffsetPadding", typeof(Thickness), typeof(SaturationBrightnessSelector),
                new UIPropertyMetadata(new Thickness(0.0)));

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(SaturationBrightnessSelector), new
                FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, HueChanged));

        public static readonly DependencyProperty SaturationOffsetProperty =
            DependencyProperty.Register("SaturationOffset", typeof(double), typeof(SaturationBrightnessSelector),
                new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(SaturationBrightnessSelector),
                new FrameworkPropertyMetadata(0.0, SaturationChanged, SaturationCoerce));

        public static readonly DependencyProperty BrightnessOffsetProperty =
            DependencyProperty.Register("BrightnessOffset", typeof(double), typeof(SaturationBrightnessSelector),
                new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(SaturationBrightnessSelector),
                new FrameworkPropertyMetadata(0.0, BrightnessChanged, BrightnessCoerce));

        public Thickness OffsetPadding
        {
            get { return (Thickness) GetValue(OffsetPaddingProperty); }
            set { SetValue(OffsetPaddingProperty, value); }
        }


        public double Hue
        {
            private get { return (double) GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }


        public double SaturationOffset
        {
            get { return (double) GetValue(SaturationOffsetProperty); }
            set { SetValue(SaturationOffsetProperty, value); }
        }


        public double Saturation
        {
            get { return (double) GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }


        public double BrightnessOffset
        {
            get { return (double) GetValue(BrightnessOffsetProperty); }
            set { SetValue(BrightnessOffsetProperty, value); }
        }


        public double Brightness
        {
            get { return (double) GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static void HueChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            var h = (SaturationBrightnessSelector) o;
            h.SetColor();
        }

        public static void SaturationChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            var h = (SaturationBrightnessSelector) o;
            h.SetSaturationOffset();
        }

        public static object SaturationCoerce(DependencyObject d, object brightness)
        {
            var v = (double) brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        public static void BrightnessChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            var h = (SaturationBrightnessSelector) o;
            h.SetBrightnessOffset();
        }

        public static object BrightnessCoerce(DependencyObject d, object brightness)
        {
            var v = (double) brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var p = e.GetPosition(this);
                Saturation = p.X/(ActualWidth - OffsetPadding.Right);
                Brightness = (ActualHeight - OffsetPadding.Bottom - p.Y)/(ActualHeight - OffsetPadding.Bottom);
                SetColor();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(this);
            Saturation = p.X/(ActualWidth - OffsetPadding.Right);
            Brightness = (ActualHeight - OffsetPadding.Bottom - p.Y)/(ActualHeight - OffsetPadding.Bottom);
            SetColor();

            Mouse.Capture(this);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var h = new LinearGradientBrush();
            h.StartPoint = new Point(0, 0);
            h.EndPoint = new Point(1, 0);
            h.GradientStops.Add(new GradientStop(Colors.White, 0.00));
            h.GradientStops.Add(new GradientStop(ColorHelper.ColorFromHsb(Hue, 1, 1), 1.0));
            dc.DrawRectangle(h, null, new Rect(0, 0, ActualWidth, ActualHeight));

            var v = new LinearGradientBrush();
            v.StartPoint = new Point(0, 0);
            v.EndPoint = new Point(0, 1);
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0, 0, 0), 1.00));
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0x80, 0, 0, 0), 0.50));
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0, 0, 0), 0.00));
            dc.DrawRectangle(v, null, new Rect(0, 0, ActualWidth, ActualHeight));

            SetSaturationOffset();
            SetBrightnessOffset();
        }

        private void SetSaturationOffset()
        {
            SaturationOffset = OffsetPadding.Left +
                               (ActualWidth - (OffsetPadding.Right + OffsetPadding.Left))*Saturation;
        }

        private void SetBrightnessOffset()
        {
            BrightnessOffset = OffsetPadding.Top +
                               (ActualHeight - (OffsetPadding.Bottom + OffsetPadding.Top) -
                                (ActualHeight - (OffsetPadding.Bottom + OffsetPadding.Top))*Brightness);
        }

        public void SetColor()
        {
            Color = ColorHelper.ColorFromHsb(Hue, Saturation, Brightness);
            //Brush = new SolidColorBrush(Color);
        }
    }
}