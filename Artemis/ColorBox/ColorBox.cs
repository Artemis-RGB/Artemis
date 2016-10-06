/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorBox
{
    [TemplatePart(Name = PartCurrentColor, Type = typeof(TextBox))]
    public class ColorBox : Control
    {
        internal const string PartCurrentColor = "PART_CurrentColor";

        public static RoutedCommand RemoveGradientStop = new RoutedCommand();
        public static RoutedCommand ReverseGradientStop = new RoutedCommand();
        public static RoutedCommand DirectionHorizontal = new RoutedCommand();
        public static RoutedCommand DirectionVertical = new RoutedCommand();
        public static RoutedCommand DirectionTopLeftBottomRight = new RoutedCommand();
        public static RoutedCommand DirectionTopRightBottomLeft = new RoutedCommand();
        internal bool BrushSetInternally;
        internal bool BrushTypeSetInternally;

        //internal bool _GradientStopSetInternally = false;
        internal bool HsbSetInternally;
        internal bool RgbSetInternally;
        internal bool UpdateBrush = true;

        static ColorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorBox), new FrameworkPropertyMetadata(typeof(ColorBox)));
        }

        internal TextBox CurrentColorTextBox { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CurrentColorTextBox = GetTemplateChild(PartCurrentColor) as TextBox;
            if (CurrentColorTextBox != null)
                CurrentColorTextBox.PreviewKeyDown += CurrentColorTextBox_PreviewKeyDown;

            CommandBindings.Add(new CommandBinding(RemoveGradientStop, RemoveGradientStop_Executed));
            CommandBindings.Add(new CommandBinding(ReverseGradientStop, ReverseGradientStop_Executed));
            CommandBindings.Add(new CommandBinding(DirectionHorizontal, DirectionHorizontal_Executed));
            CommandBindings.Add(new CommandBinding(DirectionVertical, DirectionVertical_Executed));
            CommandBindings.Add(new CommandBinding(DirectionTopLeftBottomRight, DirectionTopLeftBottomRight_Executed));
            CommandBindings.Add(new CommandBinding(DirectionTopRightBottomLeft, DirectionTopRightBottomLeft_Executed));
        }

        private void CurrentColorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var be = CurrentColorTextBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                    be.UpdateSource();
            }
        }

        private void RemoveGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((Gradients != null) && (Gradients.Count > 2))
            {
                Gradients.Remove(SelectedGradient);
                SetBrush();
            }
        }

        private void ReverseGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateBrush = false;
            BrushSetInternally = true;
            foreach (var gs in Gradients)
                gs.Offset = 1.0 - gs.Offset;
            UpdateBrush = true;
            BrushSetInternally = false;
            SetBrush();
        }

        private void DirectionHorizontal_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateBrush = false;
            BrushSetInternally = true;

            StartX = 1.0;
            StartY = 0.0;
            EndX = 1.0;
            EndY = 1.0;

            UpdateBrush = true;
            BrushSetInternally = false;
            SetBrush();
        }

        private void DirectionVertical_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateBrush = false;
            BrushSetInternally = true;

            StartX = 0.0;
            StartY = 0.0;
            EndX = 1.0;
            EndY = 0.0;

            UpdateBrush = true;
            BrushSetInternally = false;
            SetBrush();
        }

        private void DirectionTopLeftBottomRight_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateBrush = false;
            BrushSetInternally = true;

            StartX = 0.0;
            StartY = 1.0;
            EndX = 1.0;
            EndY = 0.0;

            UpdateBrush = true;
            BrushSetInternally = false;
            SetBrush();
        }

        private void DirectionTopRightBottomLeft_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateBrush = false;
            BrushSetInternally = true;

            StartX = 0.0;
            StartY = 0.0;
            EndX = 1.0;
            EndY = 1.0;

            UpdateBrush = true;
            BrushSetInternally = false;
            SetBrush();
        }

        private void InitTransform()
        {
            if ((Brush.Transform == null) || Brush.Transform.Value.IsIdentity)
            {
                BrushSetInternally = true;

                var tg = new TransformGroup();
                tg.Children.Add(new RotateTransform());
                tg.Children.Add(new ScaleTransform());
                tg.Children.Add(new SkewTransform());
                tg.Children.Add(new TranslateTransform());
                Brush.Transform = tg;

                BrushSetInternally = false;
            }
        }

        /// <summary>
        ///     Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorHsb(object o, DependencyPropertyChangedEventArgs e)
        {
            var c = (ColorBox) o;
            var n = ColorHelper.ColorFromAhsb(c.Alpha, c.Hue, c.Saturation, c.Brightness);

            c.HsbSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c.HsbSetInternally = false;
        }

        /// <summary>
        ///     Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorRgb(object o, DependencyPropertyChangedEventArgs e)
        {
            var c = (ColorBox) o;
            var n = Color.FromArgb((byte) c.A, (byte) c.R, (byte) c.G, (byte) c.B);

            c.RgbSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c.RgbSetInternally = false;
        }

        internal void SetBrush()
        {
            if (!UpdateBrush)
                return;

            BrushSetInternally = true;

            // retain old opacity
            double opacity = 1;
            TransformGroup tempTg = null;
            if (Brush != null)
            {
                opacity = Brush.Opacity;
                tempTg = Brush.Transform as TransformGroup;
            }

            switch (BrushType)
            {
                case BrushTypes.None:
                    Brush = null;
                    break;

                case BrushTypes.Solid:

                    Brush = new SolidColorBrush(Color);

                    break;

                case BrushTypes.Linear:

                    var brush = new LinearGradientBrush();
                    foreach (var g in Gradients)
                        brush.GradientStops.Add(new GradientStop(g.Color, g.Offset));
                    brush.StartPoint = new Point(StartX, StartY);
                    brush.EndPoint = new Point(EndX, EndY);
                    brush.MappingMode = MappingMode;
                    brush.SpreadMethod = SpreadMethod;
                    Brush = brush;

                    break;

                case BrushTypes.Radial:

                    var brush1 = new RadialGradientBrush();
                    foreach (var g in Gradients)
                        brush1.GradientStops.Add(new GradientStop(g.Color, g.Offset));
                    brush1.GradientOrigin = new Point(GradientOriginX, GradientOriginY);
                    brush1.Center = new Point(CenterX, CenterY);
                    brush1.RadiusX = RadiusX;
                    brush1.RadiusY = RadiusY;
                    brush1.MappingMode = MappingMode;
                    brush1.SpreadMethod = SpreadMethod;
                    Brush = brush1;

                    break;
            }

            if (BrushType != BrushTypes.None)
            {
                Brush.Opacity = opacity; // retain old opacity
                if (tempTg != null)
                    Brush.Transform = tempTg;
            }

            BrushSetInternally = false;
        }

        #region Private Properties

        private double StartX
        {
            get { return (double) GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }

        private static readonly DependencyProperty StartXProperty =
            DependencyProperty.Register("StartX", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, StartXChanged));

        private static void StartXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).StartPoint = new Point((double) args.NewValue,
                    (cp.Brush as LinearGradientBrush).StartPoint.Y);
                cp.BrushSetInternally = false;
            }
        }

        private double StartY
        {
            get { return (double) GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }

        private static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register("StartY", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.0, StartYChanged));

        private static void StartYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).StartPoint = new Point(
                    (cp.Brush as LinearGradientBrush).StartPoint.X, (double) args.NewValue);
                cp.BrushSetInternally = false;
            }
        }

        private double EndX
        {
            get { return (double) GetValue(EndXProperty); }
            set { SetValue(EndXProperty, value); }
        }

        private static readonly DependencyProperty EndXProperty =
            DependencyProperty.Register("EndX", typeof(double), typeof(ColorBox), new PropertyMetadata(0.5, EndXChanged));

        private static void EndXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).EndPoint = new Point((double) args.NewValue,
                    (cp.Brush as LinearGradientBrush).EndPoint.Y);
                cp.BrushSetInternally = false;
            }
        }

        private double EndY
        {
            get { return (double) GetValue(EndYProperty); }
            set { SetValue(EndYProperty, value); }
        }

        private static readonly DependencyProperty EndYProperty =
            DependencyProperty.Register("EndY", typeof(double), typeof(ColorBox), new PropertyMetadata(1.0, EndYChanged));

        private static void EndYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).EndPoint = new Point((cp.Brush as LinearGradientBrush).EndPoint.X,
                    (double) args.NewValue);
                cp.BrushSetInternally = false;
            }
        }


        private double GradientOriginX
        {
            get { return (double) GetValue(GradientOriginXProperty); }
            set { SetValue(GradientOriginXProperty, value); }
        }

        private static readonly DependencyProperty GradientOriginXProperty =
            DependencyProperty.Register("GradientOriginX", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, GradientOriginXChanged));

        private static void GradientOriginXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).GradientOrigin = new Point((double) args.NewValue,
                    (cp.Brush as RadialGradientBrush).GradientOrigin.Y);
                cp.BrushSetInternally = false;
            }
        }

        private double GradientOriginY
        {
            get { return (double) GetValue(GradientOriginYProperty); }
            set { SetValue(GradientOriginYProperty, value); }
        }

        private static readonly DependencyProperty GradientOriginYProperty =
            DependencyProperty.Register("GradientOriginY", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, GradientOriginYChanged));

        private static void GradientOriginYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).GradientOrigin =
                    new Point((cp.Brush as RadialGradientBrush).GradientOrigin.X, (double) args.NewValue);
                cp.BrushSetInternally = false;
            }
        }

        private double CenterX
        {
            get { return (double) GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        private static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, CenterXChanged));

        private static void CenterXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).Center = new Point((double) args.NewValue,
                    (cp.Brush as RadialGradientBrush).Center.Y);
                cp.BrushSetInternally = false;
            }
        }

        private double CenterY
        {
            get { return (double) GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        private static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, CenterYChanged));

        private static void CenterYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).Center = new Point((cp.Brush as RadialGradientBrush).Center.X,
                    (double) args.NewValue);
                cp.BrushSetInternally = false;
            }
        }

        private double RadiusX
        {
            get { return (double) GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        private static readonly DependencyProperty RadiusXProperty =
            DependencyProperty.Register("RadiusX", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, RadiusXChanged));

        private static void RadiusXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).RadiusX = (double) args.NewValue;
                cp.BrushSetInternally = false;
            }
        }

        private double RadiusY
        {
            get { return (double) GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        private static readonly DependencyProperty RadiusYProperty =
            DependencyProperty.Register("RadiusY", typeof(double), typeof(ColorBox),
                new PropertyMetadata(0.5, RadiusYChanged));

        private static void RadiusYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).RadiusY = (double) args.NewValue;
                cp.BrushSetInternally = false;
            }
        }

        private double BrushOpacity
        {
            get { return (double) GetValue(BrushOpacityProperty); }
            set { SetValue(BrushOpacityProperty, value); }
        }

        private static readonly DependencyProperty BrushOpacityProperty =
            DependencyProperty.Register("BrushOpacity", typeof(double), typeof(ColorBox), new PropertyMetadata(1.0));

        //static void BrushOpacityChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        //{
        //    ColorBox cp = property as ColorBox;
        //    cp._BrushSetInternally = true;
        //    cp.Brush.Opacity = (double)args.NewValue;
        //    cp._BrushSetInternally = false;            
        //}

        private GradientSpreadMethod SpreadMethod
        {
            get { return (GradientSpreadMethod) GetValue(SpreadMethodProperty); }
            set { SetValue(SpreadMethodProperty, value); }
        }

        private static readonly DependencyProperty SpreadMethodProperty =
            DependencyProperty.Register("SpreadMethod", typeof(GradientSpreadMethod), typeof(ColorBox),
                new PropertyMetadata(GradientSpreadMethod.Pad, SpreadMethodChanged));

        private static void SpreadMethodChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is GradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as GradientBrush).SpreadMethod = (GradientSpreadMethod) args.NewValue;
                cp.BrushSetInternally = false;
            }
        }

        private BrushMappingMode MappingMode
        {
            get { return (BrushMappingMode) GetValue(MappingModeProperty); }
            set { SetValue(MappingModeProperty, value); }
        }

        private static readonly DependencyProperty MappingModeProperty =
            DependencyProperty.Register("MappingMode", typeof(BrushMappingMode), typeof(ColorBox),
                new PropertyMetadata(BrushMappingMode.RelativeToBoundingBox, MappingModeChanged));

        private static void MappingModeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var cp = property as ColorBox;
            if (cp.Brush is GradientBrush)
            {
                cp.BrushSetInternally = true;
                (cp.Brush as GradientBrush).MappingMode = (BrushMappingMode) args.NewValue;
                cp.BrushSetInternally = false;
            }
        }

        #endregion

        #region Internal Properties

        internal ObservableCollection<GradientStop> Gradients
        {
            get { return (ObservableCollection<GradientStop>) GetValue(GradientsProperty); }
            set { SetValue(GradientsProperty, value); }
        }

        internal static readonly DependencyProperty GradientsProperty =
            DependencyProperty.Register("Gradients", typeof(ObservableCollection<GradientStop>), typeof(ColorBox));

        internal GradientStop SelectedGradient
        {
            get { return (GradientStop) GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }

        internal static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof(GradientStop), typeof(ColorBox));

        internal BrushTypes BrushType
        {
            get { return (BrushTypes) GetValue(BrushTypeProperty); }
            set { SetValue(BrushTypeProperty, value); }
        }

        internal static readonly DependencyProperty BrushTypeProperty =
            DependencyProperty.Register("BrushType", typeof(BrushTypes), typeof(ColorBox),
                new FrameworkPropertyMetadata(BrushTypes.None, BrushTypeChanged));

        private static void BrushTypeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var c = property as ColorBox;
            if (!c.BrushTypeSetInternally)
            {
                if (c.Gradients == null)
                {
                    c.Gradients = new ObservableCollection<GradientStop>();
                    c.Gradients.Add(new GradientStop(Colors.Black, 0));
                    c.Gradients.Add(new GradientStop(Colors.White, 1));
                }

                c.SetBrush();
            }
        }

        #endregion

        #region Public Properties

        public IEnumerable<Enum> SpreadMethodTypes
        {
            get
            {
                var temp = GradientSpreadMethod.Pad | GradientSpreadMethod.Reflect | GradientSpreadMethod.Repeat;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public IEnumerable<Enum> MappingModeTypes
        {
            get
            {
                var temp = BrushMappingMode.Absolute | BrushMappingMode.RelativeToBoundingBox;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public IEnumerable<Enum> AvailableBrushTypes
        {
            get
            {
                var temp = BrushTypes.None | BrushTypes.Solid | BrushTypes.Linear | BrushTypes.Radial;
                foreach (Enum value in Enum.GetValues(temp.GetType()))
                    if (temp.HasFlag(value))
                        yield return value;
            }
        }

        public Brush Brush
        {
            get { return (Brush) GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(ColorBox)
                , new FrameworkPropertyMetadata(null, BrushChanged));

        private static void BrushChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var c = property as ColorBox;
            var brush = args.NewValue as Brush;

            if (!c.BrushSetInternally)
            {
                c.BrushTypeSetInternally = true;

                if (brush == null)
                {
                    c.BrushType = BrushTypes.None;
                }
                else if (brush is SolidColorBrush)
                {
                    c.BrushType = BrushTypes.Solid;
                    c.Color = (brush as SolidColorBrush).Color;
                }
                else if (brush is LinearGradientBrush)
                {
                    var lgb = brush as LinearGradientBrush;
                    //c.Opacity = lgb.Opacity;
                    c.StartX = lgb.StartPoint.X;
                    c.StartY = lgb.StartPoint.Y;
                    c.EndX = lgb.EndPoint.X;
                    c.EndY = lgb.EndPoint.Y;
                    c.MappingMode = lgb.MappingMode;
                    c.SpreadMethod = lgb.SpreadMethod;
                    c.Gradients = new ObservableCollection<GradientStop>(lgb.GradientStops);
                    c.BrushType = BrushTypes.Linear;
                    //c.Color = lgb.GradientStops.OrderBy(x => x.Offset).Last().Color;
                    //c.SelectedGradient = lgb.GradientStops.OrderBy(x => x.Offset).Last();
                }
                else
                {
                    var rgb = brush as RadialGradientBrush;
                    c.GradientOriginX = rgb.GradientOrigin.X;
                    c.GradientOriginY = rgb.GradientOrigin.Y;
                    c.RadiusX = rgb.RadiusX;
                    c.RadiusY = rgb.RadiusY;
                    c.CenterX = rgb.Center.X;
                    c.CenterY = rgb.Center.Y;
                    c.MappingMode = rgb.MappingMode;
                    c.SpreadMethod = rgb.SpreadMethod;
                    c.Gradients = new ObservableCollection<GradientStop>(rgb.GradientStops);
                    c.BrushType = BrushTypes.Radial;
                    //c.Color = rgb.GradientStops.OrderBy(x => x.Offset).Last().Color;
                    //c.SelectedGradient = rgb.GradientStops.OrderBy(x => x.Offset).Last();
                }

                c.BrushTypeSetInternally = false;
            }
        }

        public Color Color
        {
            get { return (Color) GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorBox),
                new UIPropertyMetadata(Colors.Black, OnColorChanged));

        public static void OnColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var c = (ColorBox) o;

            if (e.NewValue is Color)
            {
                var color = (Color) e.NewValue;

                if (!c.HsbSetInternally)
                {
                    // update HSB value based on new value of color

                    double h = 0;
                    double s = 0;
                    double b = 0;
                    ColorHelper.HsbFromColor(color, ref h, ref s, ref b);

                    c.HsbSetInternally = true;

                    c.Alpha = color.A/255d;
                    c.Hue = h;
                    c.Saturation = s;
                    c.Brightness = b;

                    c.HsbSetInternally = false;
                }

                if (!c.RgbSetInternally)
                {
                    // update RGB value based on new value of color

                    c.RgbSetInternally = true;

                    c.A = color.A;
                    c.R = color.R;
                    c.G = color.G;
                    c.B = color.B;

                    c.RgbSetInternally = false;
                }

                c.RaiseColorChangedEvent((Color) e.NewValue);
            }
        }

        #endregion

        #region Color Specific Properties

        private double Hue
        {
            get { return (double) GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        private static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(ColorBox),
                new FrameworkPropertyMetadata(1.0, UpdateColorHsb, HueCoerce));

        private static object HueCoerce(DependencyObject d, object hue)
        {
            var v = (double) hue;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Brightness
        {
            get { return (double) GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        private static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(ColorBox),
                new FrameworkPropertyMetadata(0.0, UpdateColorHsb, BrightnessCoerce));

        private static object BrightnessCoerce(DependencyObject d, object brightness)
        {
            var v = (double) brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Saturation
        {
            get { return (double) GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        private static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(ColorBox),
                new FrameworkPropertyMetadata(0.0, UpdateColorHsb, SaturationCoerce));

        private static object SaturationCoerce(DependencyObject d, object saturation)
        {
            var v = (double) saturation;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Alpha
        {
            get { return (double) GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        private static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(ColorBox),
                new FrameworkPropertyMetadata(1.0, UpdateColorHsb, AlphaCoerce));

        private static object AlphaCoerce(DependencyObject d, object alpha)
        {
            var v = (double) alpha;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private int A
        {
            get { return (int) GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        private static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(int), typeof(ColorBox),
                new FrameworkPropertyMetadata(255, UpdateColorRgb, RgbCoerce));


        private int R
        {
            get { return (int) GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        private static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(int), typeof(ColorBox),
                new FrameworkPropertyMetadata(default(int), UpdateColorRgb, RgbCoerce));


        private int G
        {
            get { return (int) GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        private static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(int), typeof(ColorBox),
                new FrameworkPropertyMetadata(default(int), UpdateColorRgb, RgbCoerce));


        private int B
        {
            get { return (int) GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        private static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(int), typeof(ColorBox),
                new FrameworkPropertyMetadata(default(int), UpdateColorRgb, RgbCoerce));


        private static object RgbCoerce(DependencyObject d, object value)
        {
            var v = (int) value;
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        #endregion

        #region ColorChanged Event

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof(ColorChangedEventHandler),
                typeof(ColorBox));

        public event ColorChangedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        private void RaiseColorChangedEvent(Color color)
        {
            var newEventArgs = new ColorChangedEventArgs(ColorChangedEvent, color);
            RaiseEvent(newEventArgs);
        }

        #endregion
    }
}