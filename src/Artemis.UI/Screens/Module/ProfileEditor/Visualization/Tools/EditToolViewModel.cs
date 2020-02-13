using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Events;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class EditToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private SKPoint _dragOffset;
        private SKPoint _dragStart;
        private SKPoint _topLeft;

        public EditToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
            : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            Cursor = Cursors.Arrow;
            Update();

            profileEditorService.SelectedProfileChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            profileEditorService.ProfilePreviewUpdated += (sender, args) => Update();
        }

        public SKPath ShapePath { get; set; }
        public SKPoint ShapeAnchor { get; set; }
        public RectangleGeometry ShapeGeometry { get; set; }

        private void Update()
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            ShapePath = _layerEditorService.GetLayerPath(layer, true, true, true);
            ShapeAnchor = _layerEditorService.GetLayerAnchorPosition(layer).ToSKPoint();
            Execute.PostToUIThread(() =>
            {
                var shapeGeometry = new RectangleGeometry(_layerEditorService.GetLayerBounds(layer))
                {
                    Transform = _layerEditorService.GetLayerTransformGroup(layer)
                };
                shapeGeometry.Freeze();
                ShapeGeometry = shapeGeometry;
            });

            // Store the last top-left for easy later on
            _topLeft = _layerEditorService.GetLayerPath(layer, true, true, true).Points[0];
        }

        #region Rotation

        private bool _rotating;
        private float _previousDragAngle;

        public void RotateMouseDown(object sender, ShapeControlEventArgs e)
        {
            _rotating = true;
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
                _previousDragAngle = CalculateAngle(layer, sender, e.MouseEventArgs);
            else
                _previousDragAngle = 0;
        }

        public void RotateMouseUp(object sender, ShapeControlEventArgs e)
        {
            ProfileEditorService.UpdateSelectedProfileElement();
            _rotating = false;
        }

        public void RotateMouseMove(object sender, ShapeControlEventArgs e)
        {
            if (!_rotating || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var previousDragAngle = _previousDragAngle;
            var newRotation = CalculateAngle(layer, sender, e.MouseEventArgs);
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

            layer.RotationProperty.SetCurrentValue(newRotation, ProfileEditorService.CurrentTime);
            ProfileEditorService.UpdateProfilePreview();
        }

        #endregion

        #region Size

        private bool _isResizing;
        private SKSize _dragStartScale;

        public void ResizeMouseDown(object sender, ShapeControlEventArgs e)
        {
            if (_isResizing || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var dragStart = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();
            _dragOffset = _layerEditorService.GetDragOffset(layer, dragStart);
            _dragStart = dragStart + _dragOffset;
            _dragStartScale = layer.ScaleProperty.CurrentValue;

            _isResizing = true;
        }

        public void ResizeMouseUp(object sender, ShapeControlEventArgs e)
        {
            ProfileEditorService.UpdateSelectedProfileElement();
            _isResizing = false;
        }

        public void ResizeMouseMove(object sender, ShapeControlEventArgs e)
        {
            if (!_isResizing || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            float width, height;
            var position = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint() + _dragOffset;
            switch (e.ShapeControlPoint)
            {
                case ShapeControlPoint.TopLeft:
                    height = VerticalResize(layer, position, ResizeOrigin.Top);
                    width = HorizontalResize(layer, position, ResizeOrigin.Left);
                    break;
                case ShapeControlPoint.TopRight:
                    height = VerticalResize(layer, position, ResizeOrigin.Top);
                    width = HorizontalResize(layer, position, ResizeOrigin.Right);
                    break;
                case ShapeControlPoint.BottomRight:
                    height = VerticalResize(layer, position, ResizeOrigin.Bottom);
                    width = HorizontalResize(layer, position, ResizeOrigin.Right);
                    break;
                case ShapeControlPoint.BottomLeft:
                    height = VerticalResize(layer, position, ResizeOrigin.Bottom);
                    width = HorizontalResize(layer, position, ResizeOrigin.Left);
                    break;
                case ShapeControlPoint.TopCenter:
                    height = VerticalResize(layer, position, ResizeOrigin.Top);
                    width = layer.ScaleProperty.CurrentValue.Width;
                    break;
                case ShapeControlPoint.RightCenter:
                    width = HorizontalResize(layer, position, ResizeOrigin.Right);
                    height = layer.ScaleProperty.CurrentValue.Height;
                    break;
                case ShapeControlPoint.BottomCenter:
                    width = layer.ScaleProperty.CurrentValue.Width;
                    height = VerticalResize(layer, position, ResizeOrigin.Bottom);
                    break;
                case ShapeControlPoint.LeftCenter:
                    width = HorizontalResize(layer, position, ResizeOrigin.Left);
                    height = layer.ScaleProperty.CurrentValue.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Make the sides even if shift is held down
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && e.ShapeControlPoint < ShapeControlPoint.TopCenter)
            {
                var bounds = _layerEditorService.GetLayerBounds(layer);
                var smallestSide = Math.Min(bounds.Width * width, bounds.Height * height);
                width = (float) Math.Round(1.0 / bounds.Width * smallestSide, 2, MidpointRounding.AwayFromZero);
                height = (float) Math.Round(1.0 / bounds.Height * smallestSide, 2, MidpointRounding.AwayFromZero);
            }

            layer.ScaleProperty.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);
            ProfileEditorService.UpdateProfilePreview();
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
            var scaleToAdd = scalePerPixel * pixelsToAdd * 100f;

            return (float) Math.Round(Math.Max(0, _dragStartScale.Width + scaleToAdd), 2, MidpointRounding.AwayFromZero);
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
            var scaleToAdd = scalePerPixel * pixelsToAdd * 100f;

            return (float) Math.Round(Math.Max(0, _dragStartScale.Height + scaleToAdd), 2, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Position

        private bool _movingShape;
        private bool _movingAnchor;
        private bool _draggingHorizontally;
        private bool _draggingVertically;
        private SKPoint _dragStartAnchor;

        public void MoveMouseDown(object sender, ShapeControlEventArgs e)
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            if (e.ShapeControlPoint == ShapeControlPoint.LayerShape)
            {
                var dragStart = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();
                _dragOffset = _layerEditorService.GetDragOffset(layer, dragStart);
                _dragStart = dragStart + _dragOffset;
                _movingShape = true;
            }
            else if (e.ShapeControlPoint == ShapeControlPoint.Anchor)
            {
                var dragStartPosition = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();

                // Mouse doesn't care about rotation so get the layer path without rotation
                var path = _layerEditorService.GetLayerPath(layer, true, true, false);
                var topLeft = path.Points[0];
                // Measure from the top-left of the shape (without rotation)
                _dragOffset = topLeft + (dragStartPosition - topLeft);
                // Get the absolute layer anchor and make it relative to the unrotated shape
                _dragStartAnchor = _layerEditorService.GetLayerAnchorPosition(layer).ToSKPoint() - topLeft;
                // Ensure the anchor starts in the center of the shape it is now relative to
                _dragStartAnchor.X -= path.Bounds.Width / 2f;
                _dragStartAnchor.Y -= path.Bounds.Height / 2f;
                _movingAnchor = true;
            }
        }

        public void MoveMouseUp(object sender, ShapeControlEventArgs e)
        {
            ProfileEditorService.UpdateSelectedProfileElement();
            _movingShape = false;
            _movingAnchor = false;
        }

        public void MoveMouseMove(object sender, ShapeControlEventArgs e)
        {
            if (_movingShape)
                MoveShape(sender, e.MouseEventArgs);
            else if (_movingAnchor)
                MoveAnchor(sender, e.MouseEventArgs);
        }

        public void MoveShape(object sender, MouseEventArgs e)
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
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

        public void MoveAnchor(object sender, MouseEventArgs e)
        {
            if (!_movingAnchor || !(ProfileEditorService.SelectedProfileElement is Layer layer))
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
            var difference = _layerEditorService.GetScaledPoint(layer, _topLeft - path.Points[0], false);
            // Apply the difference so that the shape effectively stays in place
            layer.PositionProperty.SetCurrentValue(RoundPoint(layer.PositionProperty.CurrentValue + difference, 5), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        #endregion

        #region Private methods

        private SKPoint RoundPoint(SKPoint point, int decimals)
        {
            return new SKPoint((float) Math.Round(point.X, decimals, MidpointRounding.AwayFromZero), (float) Math.Round(point.Y, decimals, MidpointRounding.AwayFromZero));
        }

        private SKPoint[] UnTransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot, bool includeScale)
        {
            var counterRotatePath = new SKPath();
            counterRotatePath.AddPoly(skPoints, false);
            counterRotatePath.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue * -1, pivot.X, pivot.Y));
            if (includeScale)
                counterRotatePath.Transform(SKMatrix.MakeScale(1f / (layer.ScaleProperty.CurrentValue.Width / 100f), 1f / (layer.ScaleProperty.CurrentValue.Height / 100f)));

            return counterRotatePath.Points;
        }

        private Point GetRelativePosition(object sender, MouseEventArgs mouseEventArgs)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return mouseEventArgs.GetPosition((IInputElement) parent);
        }

        private float CalculateAngle(Layer layer, object mouseEventSender, MouseEventArgs mouseEvent)
        {
            var layerBounds = _layerEditorService.GetLayerBounds(layer);
            var start = _layerEditorService.GetLayerAnchorPosition(layer);
            var arrival = GetRelativePosition(mouseEventSender, mouseEvent);

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