using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class EditToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private bool _draggingHorizontally;
        private bool _draggingVertically;
        private SKPoint _dragOffset;
        private SKPoint _dragStart;
        private SKPoint _dragStartAnchor;
        private float _previousDragAngle;
        private SKSize _dragStartScale;
        private bool _isDragging;

        public EditToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
            : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            Cursor = Cursors.Arrow;
            Update();
            UpdateControls();

            ProfileViewModel.PanZoomViewModel.PropertyChanged += (sender, args) => UpdateControls();
            profileEditorService.SelectedProfileChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            profileEditorService.ProfilePreviewUpdated += (sender, args) => Update();
        }


        public double ControlSize { get; set; }
        public double RotateSize { get; set; }
        public Thickness ControlOffset { get; set; }
        public Thickness RotateOffset { get; set; }
        public double OutlineThickness { get; set; }

        public SKRect ShapeRectangle { get; set; }
        public SKPoint ShapeAnchor { get; set; }
        public RectangleGeometry ShapeGeometry { get; set; }
        public TransformCollection ShapeTransformCollection { get; set; }

        public SKPoint TopLeft { get; set; }
        public SKPoint TopRight { get; set; }
        public SKPoint BottomRight { get; set; }
        public SKPoint BottomLeft { get; set; }
        public SKPoint TopCenter { get; set; }
        public SKPoint RightCenter { get; set; }
        public SKPoint BottomCenter { get; set; }
        public SKPoint LeftCenter { get; set; }

        private void Update()
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            ShapeRectangle = _layerEditorService.GetShapeRenderRect(layer.LayerShape).ToSKRect();
            ShapeAnchor = _layerEditorService.GetLayerAnchor(layer, true);

            // Get a square path to use for mutation point placement
            var path = _layerEditorService.GetLayerPath(layer, true, true, true);
            TopLeft = path.Points[0];
            TopRight = path.Points[1];
            BottomRight = path.Points[2];
            BottomLeft = path.Points[3];

            TopCenter = new SKPoint((TopLeft.X + TopRight.X) / 2, (TopLeft.Y + TopRight.Y) / 2);
            RightCenter = new SKPoint((TopRight.X + BottomRight.X) / 2, (TopRight.Y + BottomRight.Y) / 2);
            BottomCenter = new SKPoint((BottomLeft.X + BottomRight.X) / 2, (BottomLeft.Y + BottomRight.Y) / 2);
            LeftCenter = new SKPoint((TopLeft.X + BottomLeft.X) / 2, (TopLeft.Y + BottomLeft.Y) / 2);

            Execute.PostToUIThread(() =>
            {
                var shapeGeometry = new RectangleGeometry(_layerEditorService.GetShapeRenderRect(layer.LayerShape))
                {
                    Transform = _layerEditorService.GetLayerTransformGroup(layer)
                };
                shapeGeometry.Freeze();
                ShapeGeometry = shapeGeometry;
                ShapeTransformCollection = _layerEditorService.GetLayerTransformGroup(layer).Children;
                ShapeTransformCollection.Freeze();
            });
        }

        private void UpdateControls()
        {
            ControlSize = Math.Max(10 / ProfileViewModel.PanZoomViewModel.Zoom, 4);
            RotateSize = ControlSize * 8;
            ControlOffset = new Thickness(ControlSize / 2 * -1, ControlSize / 2 * -1, 0, 0);
            RotateOffset = new Thickness(RotateSize / 2 * -1, RotateSize / 2 * -1, 0, 0);
            OutlineThickness = Math.Max(2 / ProfileViewModel.PanZoomViewModel.Zoom, 1);
        }

        public void ShapeEditMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            // The path starts at 0,0 so there's no simple way to get the position relative to the top-left of the path
            _dragStart = GetRelativePosition(sender, e).ToSKPoint();
            _dragStartScale = layer.SizeProperty.CurrentValue;
            _previousDragAngle = CalculateAngle(_layerEditorService.GetLayerAnchor(layer, true), _dragStart);

            // Store the original position and do a test to figure out the mouse offset
            var originalPosition = layer.PositionProperty.CurrentValue;
            var scaledDragStart = _layerEditorService.GetScaledPoint(layer, _dragStart, true);
            layer.PositionProperty.SetCurrentValue(scaledDragStart, ProfileEditorService.CurrentTime);

            // TopLeft is not updated yet and acts as a snapshot of the top-left before changing the position
            // GetLayerPath will return the updated position with all transformations applied, the difference is the offset
            _dragOffset = TopLeft - _layerEditorService.GetLayerPath(layer, true, true, true).Points[0];
            _dragStart += _dragOffset;

            // Restore the position back to before the test was done
            layer.PositionProperty.SetCurrentValue(originalPosition, ProfileEditorService.CurrentTime);

            _isDragging = true;
            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;
        }

        public void ShapeEditMouseUp(object sender, MouseButtonEventArgs e)
        {
            ProfileEditorService.UpdateSelectedProfileElement();

            _dragOffset = SKPoint.Empty;
            _dragStartAnchor = SKPoint.Empty;

            _isDragging = false;
            _draggingHorizontally = false;
            _draggingVertically = false;

            ((IInputElement) sender).ReleaseMouseCapture();
            e.Handled = true;
        }

        #region Position

        public void Move(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            // Allow the user to move the shape only horizontally or vertically when holding down shift
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Keep the X position static if dragging vertically
                if (_draggingVertically)
                    position.X = _dragStart.X;
                // Keep the Y position static if dragging horizontally
                else if (_draggingHorizontally)
                    position.Y = _dragStart.Y;
                // Snap into place only if the mouse moved atleast a full pixel
                else if (Math.Abs(position.X - _dragStart.X) > 1 || Math.Abs(position.Y - _dragStart.Y) > 1)
                {
                    // Pick between X and Y by comparing which moved the furthers from the starting point
                    _draggingHorizontally = Math.Abs(position.X - _dragStart.X) > Math.Abs(position.Y - _dragStart.Y);
                    _draggingVertically = Math.Abs(position.X - _dragStart.X) < Math.Abs(position.Y - _dragStart.Y);
                    return;
                }
            }
            // Reset both states when shift is not held down
            else
            {
                _draggingVertically = false;
                _draggingHorizontally = false;
            }

            // Scale down the resulting position and make it relative
            var scaled = _layerEditorService.GetScaledPoint(layer, position, true);
            // Update the position property
            layer.PositionProperty.SetCurrentValue(scaled, ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        #endregion

        #region Anchor

        public void AnchorEditMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
                return;

            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                var dragStartPosition = GetRelativePosition(sender, e).ToSKPoint();

                // Mouse doesn't care about rotation so get the layer path without rotation
                var path = _layerEditorService.GetLayerPath(layer, true, true, false);
                var topLeft = path.Points[0];
                // Measure from the top-left of the shape (without rotation)
                _dragOffset = topLeft + (dragStartPosition - topLeft);
                // Get the absolute layer anchor and make it relative to the unrotated shape
                _dragStartAnchor = _layerEditorService.GetLayerAnchor(layer, true) - topLeft;
                // Ensure the anchor starts in the center of the shape it is now relative to
                _dragStartAnchor.X -= path.Bounds.Width / 2f;
                _dragStartAnchor.Y -= path.Bounds.Height / 2f;
            }

            _isDragging = true;
            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;
        }

        public void AnchorMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            // The start anchor is relative to an unrotated version of the shape
            var start = _dragStartAnchor;
            // Add the current position to the start anchor to determine the new position
            var current = start + (GetRelativePosition(sender, e).ToSKPoint() - _dragOffset);
            // In order to keep the mouse movement unrotated, counter-act the active rotation
            var countered = UnTransformPoints(new[] {start, current}, layer, start, true);
            var scaled = _layerEditorService.GetScaledPoint(layer, countered[1], false);

            // Update the anchor point, this causes the shape to move
            layer.AnchorPointProperty.SetCurrentValue(RoundPoint(scaled, 5), ProfileEditorService.CurrentTime);
            // TopLeft is not updated yet and acts as a snapshot of the top-left before changing the anchor
            var path = _layerEditorService.GetLayerPath(layer, true, true, true);
            // Calculate the (scaled) difference between the old and now position
            var difference = _layerEditorService.GetScaledPoint(layer, TopLeft - path.Points[0], false);
            // Apply the difference so that the shape effectively stays in place
            layer.PositionProperty.SetCurrentValue(RoundPoint(layer.PositionProperty.CurrentValue + difference, 5), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        private SKPoint RoundPoint(SKPoint point, int decimals)
        {
            return new SKPoint((float) Math.Round(point.X, decimals, MidpointRounding.AwayFromZero), (float) Math.Round(point.Y, decimals, MidpointRounding.AwayFromZero));
        }

        #endregion

        #region Size

        public void TopLeftResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Left);
            var height = VerticalResize(layer, position, ResizeOrigin.Top);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void TopCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = layer.SizeProperty.CurrentValue.Width;
            var height = VerticalResize(layer, position, ResizeOrigin.Top);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void TopRightResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Right);
            var height = VerticalResize(layer, position, ResizeOrigin.Top);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void RightCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Right);
            var height = layer.SizeProperty.CurrentValue.Height;
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void BottomRightResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Right);
            var height = VerticalResize(layer, position, ResizeOrigin.Bottom);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void BottomCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = layer.SizeProperty.CurrentValue.Width;
            var height = VerticalResize(layer, position, ResizeOrigin.Bottom);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void BottomLeftResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Left);
            var height = VerticalResize(layer, position, ResizeOrigin.Bottom);
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void LeftCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            var width = HorizontalResize(layer, position, ResizeOrigin.Left);
            var height = layer.SizeProperty.CurrentValue.Height;
            layer.SizeProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        #endregion

        #region Rotation

        public void Rotate(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var previousDragAngle = _previousDragAngle;
            var newRotation = CalculateAngle(_layerEditorService.GetLayerAnchor(layer, true), GetRelativePosition(sender, e).ToSKPoint());
            _previousDragAngle = newRotation;

            // Allow the user to rotate the shape in increments of 5
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                newRotation = (float) Math.Round(newRotation / 5f) * 5f;

            var difference = newRotation - previousDragAngle;
            if (difference < -350)
                difference += 360;
            else if (difference > 350)
                difference -= 360;
            newRotation = layer.RotationProperty.CurrentValue + difference;

            // Round the end-result to increments of 5 as well, to avoid staying on an offset
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                newRotation = (float) Math.Round(newRotation / 5f) * 5f;
            else
                newRotation = (float) Math.Round(newRotation, 2, MidpointRounding.AwayFromZero);

            Debug.WriteLine(newRotation);
            layer.RotationProperty.SetCurrentValue(newRotation, ProfileEditorService.CurrentTime);
            ProfileEditorService.UpdateProfilePreview();
        }

        public void TopRightRotate(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;
        }

        public void BottomRightRotate(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;
        }

        public void BottomLeftRotate(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;
        }

        #endregion

        #region Private methods

        private SKPoint[] TransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot)
        {
            var counterRotatePath = new SKPath();
            counterRotatePath.AddPoly(skPoints, false);
            counterRotatePath.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue, pivot.X, pivot.Y));
            // counterRotatePath.Transform(SKMatrix.MakeScale(layer.SizeProperty.CurrentValue.Width, layer.SizeProperty.CurrentValue.Height));

            return counterRotatePath.Points;
        }

        private SKPoint[] UnTransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot, bool includeScale)
        {
            var counterRotatePath = new SKPath();
            counterRotatePath.AddPoly(skPoints, false);
            counterRotatePath.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue * -1, pivot.X, pivot.Y));
            if (includeScale)
                counterRotatePath.Transform(SKMatrix.MakeScale(1f / layer.SizeProperty.CurrentValue.Width, 1f / layer.SizeProperty.CurrentValue.Height));

            return counterRotatePath.Points;
        }

        private Point GetRelativePosition(object sender, MouseEventArgs mouseEventArgs)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return mouseEventArgs.GetPosition((IInputElement) parent);
        }

        private float HorizontalResize(Layer layer, SKPoint position, ResizeOrigin origin)
        {
            // Apply rotation to the mouse
            var points = UnTransformPoints(new[] {position, _dragStart}, layer, ShapeAnchor, false);
            position = points[0];
            var dragStart = points[1];

            var shapePath = _layerEditorService.GetLayerPath(layer, true, false, false);
            var scalePerPixel = 1f / shapePath.Bounds.Width;
            var anchorDistance = origin == ResizeOrigin.Left
                ? shapePath.Bounds.Left - ShapeAnchor.X
                : shapePath.Bounds.Right - ShapeAnchor.X;
            var anchorOffset = anchorDistance / shapePath.Bounds.Width;

            var pixelsToAdd = (position - dragStart).X / anchorOffset;
            var scaleToAdd = scalePerPixel * pixelsToAdd;

            return Math.Max(0.001f, _dragStartScale.Width + scaleToAdd);
        }

        private float VerticalResize(Layer layer, SKPoint position, ResizeOrigin origin)
        {
            // Apply rotation to the mouse
            var points = UnTransformPoints(new[] {position, _dragStart}, layer, ShapeAnchor, false);
            position = points[0];
            var dragStart = points[1];

            var shapePath = _layerEditorService.GetLayerPath(layer, true, false, false);
            var scalePerPixel = 1f / shapePath.Bounds.Height;
            var anchorDistance = origin == ResizeOrigin.Top
                ? shapePath.Bounds.Top - ShapeAnchor.Y
                : shapePath.Bounds.Bottom - ShapeAnchor.Y;
            var anchorOffset = anchorDistance / shapePath.Bounds.Height;

            var pixelsToAdd = (position - dragStart).Y / anchorOffset;
            var scaleToAdd = scalePerPixel * pixelsToAdd;

            return Math.Max(0.001f, _dragStartScale.Height + scaleToAdd);
        }

        private float CalculateAngle(SKPoint start, SKPoint arrival)
        {
            var radian = (float) Math.Atan2(start.Y - arrival.Y, start.X - arrival.X);
            var angle = radian * (180f / (float) Math.PI);
            if (angle < 0f)
                angle += 360f;

            return angle;
        }

        #endregion
    }

    internal enum ResizeOrigin
    {
        Left,
        Right,
        Top,
        Bottom
    }
}