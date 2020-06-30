using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Extensions;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.Screens.Shared
{
    public class PanZoomViewModel : PropertyChangedBase
    {
        private Point? _lastPanPosition;
        private double _zoom = 1;
        private double _panX;
        private double _panY;
        private double _canvasWidth;
        private double _canvasHeight;
        private bool _limitToZero;

        public Point? LastPanPosition
        {
            get => _lastPanPosition;
            set => SetAndNotify(ref _lastPanPosition, value);
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                if (!SetAndNotify(ref _zoom, value)) return;
                NotifyOfPropertyChange(nameof(ZoomPercentage));
            }
        }

        public double ZoomPercentage
        {
            get => Zoom * 100;
            set => SetZoomFromPercentage(value);
        }

        public double PanX
        {
            get => _panX;
            set
            {
                if (!SetAndNotify(ref _panX, value)) return;
                NotifyOfPropertyChange(nameof(BackgroundViewport));
            }
        }

        public double PanY
        {
            get => _panY;
            set
            {
                if (!SetAndNotify(ref _panY, value)) return;
                NotifyOfPropertyChange(nameof(BackgroundViewport));
            }
        }

        public double CanvasWidth
        {
            get => _canvasWidth;
            set => SetAndNotify(ref _canvasWidth, value);
        }

        public double CanvasHeight
        {
            get => _canvasHeight;
            set => SetAndNotify(ref _canvasHeight, value);
        }

        public bool LimitToZero
        {
            get => _limitToZero;
            set => SetAndNotify(ref _limitToZero, value);
        }

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

            // Limit to a min of 0.1 and a max of 2.5 (10% - 250% in the view)
            Zoom = Math.Max(0.1, Math.Min(2.5, Zoom));

            // Update the PanX/Y to enable zooming relative to cursor
            if (LimitToZero)
            {
                PanX = Math.Min(0, absoluteX - relative.X * Zoom);
                PanY = Math.Min(0, absoluteY - relative.Y * Zoom);
            }
            else
            {
                PanX = absoluteX - relative.X * Zoom;
                PanY = absoluteY - relative.Y * Zoom;
            }
        }

        public void ProcessMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                LastPanPosition = null;
                return;
            }

            if (LastPanPosition == null)
                LastPanPosition = e.GetPosition((IInputElement) sender);

            var position = e.GetPosition((IInputElement) sender);
            var delta = LastPanPosition - position;

            if (LimitToZero)
            {
                PanX = Math.Min(0, PanX - delta.Value.X);
                PanY = Math.Min(0, PanY - delta.Value.Y);
            }
            else
            {
                PanX -= delta.Value.X;
                PanY -= delta.Value.Y;
            }

            LastPanPosition = position;
        }

        public void Reset()
        {
            Zoom = 1;
            PanX = 0;
            PanY = 0;
        }

        public Rect TransformContainingRect(Rectangle rect)
        {
            return TransformContainingRect(rect.ToWindowsRect(1));
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