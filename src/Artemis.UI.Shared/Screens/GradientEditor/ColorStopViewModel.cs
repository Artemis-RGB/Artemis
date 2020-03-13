using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    public class ColorStopViewModel : PropertyChangedBase
    {
        private readonly GradientEditorViewModel _gradientEditorViewModel;

        public ColorStopViewModel(GradientEditorViewModel gradientEditorViewModel)
        {
            _gradientEditorViewModel = gradientEditorViewModel;
        }

        public ColorGradientStop ColorStop { get; set; }
        public double Offset => ColorStop.Position * _gradientEditorViewModel.PreviewWidth;
        public SKColor Color => ColorStop.Color;

        #region Movement

        private double _mouseDownOffset;
        private DateTime _mouseDownTime;

        public void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = GetPositionInPreview(sender, e);
            ((IInputElement) sender).CaptureMouse();
            _mouseDownOffset = Offset - position.X;
            _mouseDownTime = DateTime.Now;
        }

        public void StopMouseUp(object sender, MouseButtonEventArgs e)
        {
            // On regular click, select this color stop
            if (DateTime.Now - _mouseDownTime <= TimeSpan.FromMilliseconds(250))
            {
            }

            ((IInputElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }

        public void StopMouseMove(object sender, MouseEventArgs e)
        {
            if (!((IInputElement) sender).IsMouseCaptured)
                return;

            var position = GetPositionInPreview(sender, e);
            // Scale down with a precision of 3 decimals
            var newPosition = Math.Round((position.X + _mouseDownOffset) / _gradientEditorViewModel.PreviewWidth, 3, MidpointRounding.AwayFromZero);
            // Limit from 0.0 to 1.0
            newPosition = Math.Min(1, Math.Max(0, newPosition));

            ColorStop.Position = (float) newPosition;
            NotifyOfPropertyChange(() => Offset);
            NotifyOfPropertyChange(() => Color);
        }

        private Point GetPositionInPreview(object sender, MouseEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return e.GetPosition((IInputElement) parent);
        }

        #endregion

        public void Update(ColorGradientStop colorStop)
        {
            ColorStop = colorStop;
            NotifyOfPropertyChange(() => Offset);
            NotifyOfPropertyChange(() => Color);
        }
    }
}