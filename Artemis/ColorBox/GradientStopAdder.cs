/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorBox
{
    public class GradientStopAdder : Button
    {
        public static readonly DependencyProperty ColorBoxProperty =
            DependencyProperty.Register("ColorBox", typeof(ColorBox), typeof(GradientStopAdder));

        public ColorBox ColorBox
        {
            get { return (ColorBox) GetValue(ColorBoxProperty); }
            set { SetValue(ColorBoxProperty, value); }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (e.Source is GradientStopAdder && (ColorBox != null))
            {
                var btn = e.Source as Button;

                var gs = new GradientStop();
                gs.Offset = Mouse.GetPosition(btn).X/btn.ActualWidth;
                //_gs.Color = this.ColorBox.Color;
                gs.Color = GetColorFromImage(e.GetPosition(this));
                ColorBox.Gradients.Add(gs);
                ColorBox.SelectedGradient = gs;
                ColorBox.Color = gs.Color;
                ColorBox.SetBrush();
            }
        }

        private Color GetColorFromImage(Point p)
        {
            try
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(this);
                var rtb = new RenderTargetBitmap((int) bounds.Width, (int) bounds.Height, 96, 96, PixelFormats.Default);
                rtb.Render(this);

                byte[] arr;
                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                using (var stream = new MemoryStream())
                {
                    png.Save(stream);
                    arr = stream.ToArray();
                }

                BitmapSource bitmap = BitmapFrame.Create(new MemoryStream(arr));

                var pixels = new byte[4];
                var cb = new CroppedBitmap(bitmap, new Int32Rect((int) p.X, (int) p.Y, 1, 1));
                cb.CopyPixels(pixels, 4, 0);
                return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            }
            catch (Exception)
            {
                return ColorBox.Color;
            }
        }
    }
}