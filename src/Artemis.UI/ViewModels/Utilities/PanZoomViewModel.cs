using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Stylet;

namespace Artemis.UI.ViewModels.Utilities
{
    public class PanZoomViewModel : PropertyChangedBase
    {
        private Point? _lastPanPosition;
        public double Zoom { get; set; } = 1;

        public double ZoomPercentage
        {
            get => Zoom * 100;
            set => SetZoomFromPercentage(value);
        }

        public double PanX { get; set; }
        public double PanY { get; set; }

        public Rect BackgroundViewport => new Rect(PanX, PanY, 20, 20);

        public void ProcessMouseScroll(object sender, MouseWheelEventArgs e)
        {
            var relative = GetRelativeMousePosition(sender, e);
            var absoluteX = relative.X * Zoom + PanX;
            var absoluteY = relative.Y * Zoom + PanY;

            if (e.Delta > 0)
                Zoom *= 1.1;
            else
                Zoom *= 0.9;

            // Limit to a min of 0.1 and a max of 4 (10% - 400% in the view)
            Zoom = Math.Max(0.1, Math.Min(4, Zoom));

            // Update the PanX/Y to enable zooming relative to cursor
            PanX = Math.Min(0, absoluteX - relative.X * Zoom);
            PanY = Math.Min(0, absoluteY - relative.Y * Zoom);
        }

        public void ProcessMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _lastPanPosition = null;
                return;
            }

            if (_lastPanPosition == null)
                _lastPanPosition = e.GetPosition((IInputElement) sender);

            var position = e.GetPosition((IInputElement) sender);
            var delta = _lastPanPosition - position;

            PanX = Math.Min(0, PanX - delta.Value.X);
            PanY = Math.Min(0, PanY - delta.Value.Y);
            
            _lastPanPosition = position;
        }

        public void Reset()
        {
            Zoom = 1;
            PanX = 0;
            PanY = 0;
        }

        public Rect TransformContainingRect(Rect rect)
        {
            // Create the same transform group the view is using
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(Zoom, Zoom));
            transformGroup.Children.Add(new TranslateTransform(PanX, PanY));

            // Apply it to the device rect
            return transformGroup.TransformBounds(rect);
        }

        public Point GetRelativeMousePosition(object container, MouseEventArgs e)
        {
            // Get the mouse position relative to the panned / zoomed panel, not very MVVM but I can't find a better way
            return e.GetPosition(((Panel) container).Children[0]);
        }

        private void SetZoomFromPercentage(double value)
        {
            var newZoom = value / 100;
            // Focus towards the center of the zoomed area
            PanX += newZoom - Zoom;
            PanY += newZoom - Zoom;
            Zoom = value / 100;
        }
    }
}