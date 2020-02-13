using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Events;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.UserControls
{
    /// <summary>
    ///     Interaction logic for LayerShapeControl.xaml
    /// </summary>
    public partial class LayerShapeControl : UserControl
    {
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(LayerShapeControl),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.None, ZoomChanged));

        public static readonly DependencyProperty ShapePathProperty = DependencyProperty.Register(nameof(ShapePath), typeof(SKPath), typeof(LayerShapeControl),
            new FrameworkPropertyMetadata(default(SKPath), FrameworkPropertyMetadataOptions.None, ShapePathChanged));

        public static readonly DependencyProperty ShapeAnchorProperty = DependencyProperty.Register(nameof(ShapeAnchor), typeof(SKPoint), typeof(LayerShapeControl),
            new FrameworkPropertyMetadata(default(SKPoint), FrameworkPropertyMetadataOptions.None, ShapeAnchorChanged));

        public static readonly DependencyProperty ShapeGeometryProperty = DependencyProperty.Register(nameof(ShapeGeometry), typeof(Geometry), typeof(LayerShapeControl),
            new FrameworkPropertyMetadata(default(Geometry), FrameworkPropertyMetadataOptions.None, ShapeGeometryChanged));

        public LayerShapeControl()
        {
            InitializeComponent();
            UpdatePositions();
            UpdateDimensions();
        }

        public double Zoom
        {
            get => (double) GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public SKPath ShapePath
        {
            get => (SKPath) GetValue(ShapePathProperty);
            set => SetValue(ShapePathProperty, value);
        }

        public SKPoint ShapeAnchor
        {
            get => (SKPoint) GetValue(ShapeAnchorProperty);
            set => SetValue(ShapeAnchorProperty, value);
        }

        public Geometry ShapeGeometry
        {
            get => (Geometry) GetValue(ShapeGeometryProperty);
            set => SetValue(ShapeGeometryProperty, value);
        }

        public void UpdateDimensions()
        {
            if (Zoom == 0)
                return;

            // Rotation controls
            UpdateRotateDimensions(RotateTopLeft);
            UpdateRotateDimensions(RotateTopRight);
            UpdateRotateDimensions(RotateBottomRight);
            UpdateRotateDimensions(RotateBottomLeft);

            // Size controls
            UpdateResizeDimensions(ResizeTopCenter);
            UpdateResizeDimensions(ResizeRightCenter);
            UpdateResizeDimensions(ResizeBottomCenter);
            UpdateResizeDimensions(ResizeLeftCenter);
            UpdateResizeDimensions(ResizeTopLeft);
            UpdateResizeDimensions(ResizeTopRight);
            UpdateResizeDimensions(ResizeBottomRight);
            UpdateResizeDimensions(ResizeBottomLeft);

            // Anchor point
            UpdateResizeDimensions(AnchorPoint);

            // Layer outline
            LayerShapeOutline.StrokeThickness = Math.Max(2 / Zoom, 1);
        }

        private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layerShapeControl = (LayerShapeControl) d;

            layerShapeControl.SetCurrentValue(ZoomProperty, e.NewValue);
            layerShapeControl.UpdateDimensions();
        }

        private static void ShapePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layerShapeControl = (LayerShapeControl) d;

            layerShapeControl.SetCurrentValue(ShapePathProperty, e.NewValue);
            layerShapeControl.UpdatePositions();
        }

        private static void ShapeAnchorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layerShapeControl = (LayerShapeControl) d;

            layerShapeControl.SetCurrentValue(ShapeAnchorProperty, e.NewValue);
            layerShapeControl.UpdateShapeAnchor();
        }

        private static void ShapeGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layerShapeControl = (LayerShapeControl) d;

            layerShapeControl.SetCurrentValue(ShapeGeometryProperty, e.NewValue);
            layerShapeControl.UpdateShapeGeometry();
        }

        private void UpdateShapeGeometry()
        {
            LayerShapeOutline.Data = ShapeGeometry;
        }

        private void UpdateShapeAnchor()
        {
            UpdateControlPosition(AnchorPoint, ShapeAnchor);
        }

        private void UpdatePositions()
        {
            if (ShapePath == null)
                return;

            // Rotation controls
            UpdateControlPosition(RotateTopLeft, ShapePath.Points[0]);
            UpdateControlPosition(RotateTopRight, ShapePath.Points[1]);
            UpdateControlPosition(RotateBottomRight, ShapePath.Points[2]);
            UpdateControlPosition(RotateBottomLeft, ShapePath.Points[3]);

            // Size controls
            UpdateControlPosition(ResizeTopCenter, ShapePath.Points[0], ShapePath.Points[1]);
            UpdateControlPosition(ResizeRightCenter, ShapePath.Points[1], ShapePath.Points[2]);
            UpdateControlPosition(ResizeBottomCenter, ShapePath.Points[2], ShapePath.Points[3]);
            UpdateControlPosition(ResizeLeftCenter, ShapePath.Points[3], ShapePath.Points[0]);
            UpdateControlPosition(ResizeTopLeft, ShapePath.Points[0]);
            UpdateControlPosition(ResizeTopRight, ShapePath.Points[1]);
            UpdateControlPosition(ResizeBottomRight, ShapePath.Points[2]);
            UpdateControlPosition(ResizeBottomLeft, ShapePath.Points[3]);
        }

        #region Helpers

        private void UpdateControlPosition(UIElement control, SKPoint point)
        {
            if (float.IsInfinity(point.X) || float.IsInfinity(point.Y))
                return;
            Canvas.SetLeft(control, point.X);
            Canvas.SetTop(control, point.Y);
        }

        private void UpdateControlPosition(UIElement control, SKPoint point1, SKPoint point2)
        {
            var point = new SKPoint((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
            UpdateControlPosition(control, point);
        }

        private void UpdateRotateDimensions(FrameworkElement rotateControl)
        {
            var controlSize = Math.Max(10 / Zoom, 4);
            var rotateSize = controlSize * 8;

            rotateControl.Width = rotateSize;
            rotateControl.Height = rotateSize;
            rotateControl.Margin = new Thickness(rotateSize / 2 * -1, rotateSize / 2 * -1, 0, 0);
        }

        private void UpdateResizeDimensions(FrameworkElement resizeControl)
        {
            var controlSize = Math.Max(10 / Zoom, 4);

            resizeControl.Width = controlSize;
            resizeControl.Height = controlSize;
            resizeControl.Margin = new Thickness(controlSize / 2 * -1, controlSize / 2 * -1, 0, 0);
        }

        #endregion

        #region Event handlers

        private void RotationOnMouseDown(object sender, MouseEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;

            if (ReferenceEquals(sender, RotateTopLeft))
                OnRotateMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, RotateTopRight))
                OnRotateMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, RotateBottomRight))
                OnRotateMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, RotateBottomLeft))
                OnRotateMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));
        }

        private void RotationOnMouseUp(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, RotateTopLeft))
                OnRotateMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, RotateTopRight))
                OnRotateMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, RotateBottomRight))
                OnRotateMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, RotateBottomLeft))
                OnRotateMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));

            ((IInputElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }

        private void RotationOnMouseMove(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, RotateTopLeft))
                OnRotateMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, RotateTopRight))
                OnRotateMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, RotateBottomRight))
                OnRotateMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, RotateBottomLeft))
                OnRotateMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));
        }

        private void ResizeOnMouseDown(object sender, MouseEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;

            if (ReferenceEquals(sender, ResizeTopCenter))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.TopCenter));
            else if (ReferenceEquals(sender, ResizeRightCenter))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.RightCenter));
            else if (ReferenceEquals(sender, ResizeBottomCenter))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.BottomCenter));
            else if (ReferenceEquals(sender, ResizeLeftCenter))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.LeftCenter));
            else if (ReferenceEquals(sender, ResizeTopLeft))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, ResizeTopRight))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, ResizeBottomRight))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, ResizeBottomLeft))
                OnResizeMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));
        }

        private void ResizeOnMouseUp(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, ResizeTopCenter))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.TopCenter));
            else if (ReferenceEquals(sender, ResizeRightCenter))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.RightCenter));
            else if (ReferenceEquals(sender, ResizeBottomCenter))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.BottomCenter));
            else if (ReferenceEquals(sender, ResizeLeftCenter))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.LeftCenter));
            else if (ReferenceEquals(sender, ResizeTopLeft))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, ResizeTopRight))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, ResizeBottomRight))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, ResizeBottomLeft))
                OnResizeMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));

            ((IInputElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }

        private void ResizeOnMouseMove(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, ResizeTopCenter))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.TopCenter));
            else if (ReferenceEquals(sender, ResizeRightCenter))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.RightCenter));
            else if (ReferenceEquals(sender, ResizeBottomCenter))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.BottomCenter));
            else if (ReferenceEquals(sender, ResizeLeftCenter))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.LeftCenter));
            else if (ReferenceEquals(sender, ResizeTopLeft))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.TopLeft));
            else if (ReferenceEquals(sender, ResizeTopRight))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.TopRight));
            else if (ReferenceEquals(sender, ResizeBottomRight))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.BottomRight));
            else if (ReferenceEquals(sender, ResizeBottomLeft))
                OnResizeMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.BottomLeft));
        }

        private void MoveOnMouseDown(object sender, MouseEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;

            if (ReferenceEquals(sender, LayerShapeOutline))
                OnMoveMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.LayerShape));
            else if (ReferenceEquals(sender, AnchorPoint))
                OnMoveMouseDown(new ShapeControlEventArgs(e, ShapeControlPoint.Anchor));
        }

        private void MoveOnMouseUp(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, LayerShapeOutline))
                OnMoveMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.LayerShape));
            else if (ReferenceEquals(sender, AnchorPoint))
                OnMoveMouseUp(new ShapeControlEventArgs(e, ShapeControlPoint.Anchor));

            ((IInputElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }

        private void MoveOnMouseMove(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, LayerShapeOutline))
                OnMoveMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.LayerShape));
            else if (ReferenceEquals(sender, AnchorPoint))
                OnMoveMouseMove(new ShapeControlEventArgs(e, ShapeControlPoint.Anchor));
        }

        #endregion

        #region Events

        public event EventHandler<ShapeControlEventArgs> RotateMouseDown;
        public event EventHandler<ShapeControlEventArgs> RotateMouseUp;
        public event EventHandler<ShapeControlEventArgs> RotateMouseMove;

        public event EventHandler<ShapeControlEventArgs> ResizeMouseDown;
        public event EventHandler<ShapeControlEventArgs> ResizeMouseUp;
        public event EventHandler<ShapeControlEventArgs> ResizeMouseMove;

        public event EventHandler<ShapeControlEventArgs> MoveMouseDown;
        public event EventHandler<ShapeControlEventArgs> MoveMouseUp;
        public event EventHandler<ShapeControlEventArgs> MoveMouseMove;

        protected virtual void OnMoveMouseMove(ShapeControlEventArgs e)
        {
            MoveMouseMove?.Invoke(this, e);
        }

        protected virtual void OnMoveMouseUp(ShapeControlEventArgs e)
        {
            MoveMouseUp?.Invoke(this, e);
        }

        protected virtual void OnMoveMouseDown(ShapeControlEventArgs e)
        {
            MoveMouseDown?.Invoke(this, e);
        }

        protected virtual void OnResizeMouseMove(ShapeControlEventArgs e)
        {
            ResizeMouseMove?.Invoke(this, e);
        }

        protected virtual void OnResizeMouseUp(ShapeControlEventArgs e)
        {
            ResizeMouseUp?.Invoke(this, e);
        }

        protected virtual void OnResizeMouseDown(ShapeControlEventArgs e)
        {
            ResizeMouseDown?.Invoke(this, e);
        }

        protected virtual void OnRotateMouseMove(ShapeControlEventArgs e)
        {
            RotateMouseMove?.Invoke(this, e);
        }

        protected virtual void OnRotateMouseUp(ShapeControlEventArgs e)
        {
            RotateMouseUp?.Invoke(this, e);
        }

        protected virtual void OnRotateMouseDown(ShapeControlEventArgs e)
        {
            RotateMouseDown?.Invoke(this, e);
        }

        #endregion
    }
}