/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorBox
{
    public class GradientStopSlider : Slider
    {
        public static readonly DependencyProperty ColorBoxProperty =
            DependencyProperty.Register("ColorBox", typeof(ColorBox), typeof(GradientStopSlider));

        public static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof(GradientStop), typeof(GradientStopSlider));

        public ColorBox ColorBox
        {
            get { return (ColorBox) GetValue(ColorBoxProperty); }
            set { SetValue(ColorBoxProperty, value); }
        }

        public GradientStop SelectedGradient
        {
            get { return (GradientStop) GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (ColorBox != null)
            {
                ColorBox.BrushSetInternally = true;
                ColorBox.UpdateBrush = false;

                ColorBox.SelectedGradient = SelectedGradient;
                ColorBox.Color = SelectedGradient.Color;

                ColorBox.UpdateBrush = true;
                //this.ColorBox._BrushSetInternally = false;

                //e.Handled = true;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            if (ColorBox != null)
            {
                //this.ColorBox._HSBSetInternally = true;
                //this.ColorBox._RGBSetInternally = true;
                ColorBox.BrushSetInternally = true;
                ColorBox.SetBrush();
                ColorBox.HsbSetInternally = false;
                //this.ColorBox._RGBSetInternally = false;
                //this.ColorBox._BrushSetInternally = false;
            }
        }
    }
}