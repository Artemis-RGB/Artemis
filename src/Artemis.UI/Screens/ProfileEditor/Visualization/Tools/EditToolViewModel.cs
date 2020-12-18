using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Events;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public class EditToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private SKPoint _dragOffset;
        private SKPoint _dragStart;
        private SKPoint _shapeAnchor;
        private RectangleGeometry _shapeGeometry;
        private SKPath _shapePath;
        private SKPoint _topLeft;

        public EditToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
            : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            Cursor = Cursors.Arrow;
            Update();

            profileEditorService.ProfileSelected += (sender, args) => Update();
            profileEditorService.ProfileElementSelected += (sender, args) => Update();
            profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            profileEditorService.ProfilePreviewUpdated += (sender, args) => Update();
        }

        public SKPath ShapePath
        {
            get => _shapePath;
            set => SetAndNotify(ref _shapePath, value);
        }

        public SKPoint ShapeAnchor
        {
            get => _shapeAnchor;
            set => SetAndNotify(ref _shapeAnchor, value);
        }

        public RectangleGeometry ShapeGeometry
        {
            get => _shapeGeometry;
            set => SetAndNotify(ref _shapeGeometry, value);
        }

        private void Update()
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            ShapePath = _layerEditorService.GetLayerPath(layer, true, true, true);
            ShapeAnchor = _layerEditorService.GetLayerAnchorPosition(layer).ToSKPoint();
            Execute.PostToUIThread(() =>
            {
                RectangleGeometry shapeGeometry = new(_layerEditorService.GetLayerBounds(layer))
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

            float previousDragAngle = _previousDragAngle;
            float newRotation = CalculateAngle(layer, sender, e.MouseEventArgs);
            _previousDragAngle = newRotation;

            // Allow the user to rotate the shape in increments of 5
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                newRotation = (float) Math.Round(newRotation / 5f) * 5f;

            float difference = newRotation - previousDragAngle;
            if (difference < -350)
                difference += 360;
            else if (difference > 350)
                difference -= 360;

            newRotation = layer.Transform.Rotation.CurrentValue + difference;

            // Round the end-result to increments of 5 as well, to avoid staying on an offset
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                newRotation = (float) Math.Round(newRotation / 5f) * 5f;
            else
                newRotation = (float) Math.Round(newRotation, 2, MidpointRounding.AwayFromZero);
            layer.Transform.Rotation.SetCurrentValue(newRotation, ProfileEditorService.CurrentTime);
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

            SKPoint dragStart = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();
            _dragOffset = _layerEditorService.GetDragOffset(layer, dragStart);
            _dragStart = dragStart + _dragOffset;
            _dragStartScale = layer.Transform.Scale.CurrentValue;

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
            SKPoint position = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint() + _dragOffset;
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
                    width = layer.Transform.Scale.CurrentValue.Width;
                    break;
                case ShapeControlPoint.RightCenter:
                    width = HorizontalResize(layer, position, ResizeOrigin.Right);
                    height = layer.Transform.Scale.CurrentValue.Height;
                    break;
                case ShapeControlPoint.BottomCenter:
                    width = layer.Transform.Scale.CurrentValue.Width;
                    height = VerticalResize(layer, position, ResizeOrigin.Bottom);
                    break;
                case ShapeControlPoint.LeftCenter:
                    width = HorizontalResize(layer, position, ResizeOrigin.Left);
                    height = layer.Transform.Scale.CurrentValue.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }

            // Make the sides even if shift is held down
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && e.ShapeControlPoint < ShapeControlPoint.TopCenter)
            {
                Rect bounds = _layerEditorService.GetLayerBounds(layer);
                double smallestSide = Math.Min(bounds.Width * width, bounds.Height * height);
                width = (float) Math.Round(1.0 / bounds.Width * smallestSide, 2, MidpointRounding.AwayFromZero);
                height = (float) Math.Round(1.0 / bounds.Height * smallestSide, 2, MidpointRounding.AwayFromZero);
            }

            layer.Transform.Scale.SetCurrentValue(new SKSize(width, height), ProfileEditorService.CurrentTime);
            ProfileEditorService.UpdateProfilePreview();
        }

        private float HorizontalResize(Layer layer, SKPoint position, ResizeOrigin origin)
        {
            // Apply rotation to the mouse
            SKPoint[] points = UnTransformPoints(new[] {position, _dragStart}, layer, ShapeAnchor, false);
            position = points[0];
            SKPoint dragStart = points[1];

            SKPath shapePath = _layerEditorService.GetLayerPath(layer, true, false, false);
            float scalePerPixel = 1f / shapePath.Bounds.Width;
            float anchorDistance = origin == ResizeOrigin.Left
                ? shapePath.Bounds.Left - ShapeAnchor.X
                : shapePath.Bounds.Right - ShapeAnchor.X;

            // Don't resize if the distance to the anchor is too small, that'll result in a NaN value
            // this might happen if the anchor is at X -0.5 and the drag handle is on the left side or vice versa
            if (Math.Abs(anchorDistance) < 0.001)
                return _dragStartScale.Width;

            float anchorOffset = anchorDistance / shapePath.Bounds.Width;

            float pixelsToAdd = (position - dragStart).X / anchorOffset;
            float scaleToAdd = scalePerPixel * pixelsToAdd * 100f;

            return (float) Math.Round(Math.Max(0, _dragStartScale.Width + scaleToAdd), 2, MidpointRounding.AwayFromZero);
        }

        private float VerticalResize(Layer layer, SKPoint position, ResizeOrigin origin)
        {
            // Apply rotation to the mouse
            SKPoint[] points = UnTransformPoints(new[] {position, _dragStart}, layer, ShapeAnchor, false);
            position = points[0];
            SKPoint dragStart = points[1];

            SKPath shapePath = _layerEditorService.GetLayerPath(layer, true, false, false);
            float scalePerPixel = 1f / shapePath.Bounds.Height;
            float anchorDistance = origin == ResizeOrigin.Top
                ? shapePath.Bounds.Top - ShapeAnchor.Y
                : shapePath.Bounds.Bottom - ShapeAnchor.Y;

            // Don't resize if the distance to the anchor is too small, that'll result in a NaN value
            // this might happen if the anchor is at Y -0.5 and the drag handle is on the top side or vice versa
            if (Math.Abs(anchorDistance) < 0.001)
                return _dragStartScale.Width;

            float anchorOffset = anchorDistance / shapePath.Bounds.Height;

            float pixelsToAdd = (position - dragStart).Y / anchorOffset;
            float scaleToAdd = scalePerPixel * pixelsToAdd * 100f;

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
                SKPoint dragStart = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();
                _dragOffset = _layerEditorService.GetDragOffset(layer, dragStart);
                _dragStart = dragStart + _dragOffset;
                _movingShape = true;
            }
            else if (e.ShapeControlPoint == ShapeControlPoint.Anchor)
            {
                SKPoint dragStartPosition = GetRelativePosition(sender, e.MouseEventArgs).ToSKPoint();

                // Mouse doesn't care about rotation so get the layer path without rotation
                SKPath path = _layerEditorService.GetLayerPath(layer, true, true, false);
                SKPoint topLeft = path.Points[0];
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

            SKPoint position = GetRelativePosition(sender, e).ToSKPoint() + _dragOffset;
            // Allow the user to move the shape only horizontally or vertically when holding down shift
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Keep the X position static if dragging vertically
                if (_draggingVertically)
                {
                    position.X = _dragStart.X;
                }
                // Keep the Y position static if dragging horizontally
                else if (_draggingHorizontally)
                {
                    position.Y = _dragStart.Y;
                }
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
            SKPoint scaled = _layerEditorService.GetScaledPoint(layer, position, true);
            // Round and update the position property
            layer.Transform.Position.SetCurrentValue(RoundPoint(scaled, 3), ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void MoveAnchor(object sender, MouseEventArgs e)
        {
            if (!_movingAnchor || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            // The start anchor is relative to an unrotated version of the shape
            SKPoint start = _dragStartAnchor;
            // Add the current position to the start anchor to determine the new position
            SKPoint current = start + (GetRelativePosition(sender, e).ToSKPoint() - _dragOffset);
            // In order to keep the mouse movement unrotated, counter-act the active rotation
            SKPoint[] countered = UnTransformPoints(new[] {start, current}, layer, start, true);

            // If shift is held down, round down to 1 decimal to allow moving the anchor in big increments
            int decimals = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? 1 : 3;
            SKPoint scaled = RoundPoint(_layerEditorService.GetScaledPoint(layer, countered[1], false), decimals);

            // Update the anchor point, this causes the shape to move
            layer.Transform.AnchorPoint.SetCurrentValue(scaled, ProfileEditorService.CurrentTime);
            // TopLeft is not updated yet and acts as a snapshot of the top-left before changing the anchor
            SKPath path = _layerEditorService.GetLayerPath(layer, true, true, true);
            // Calculate the (scaled) difference between the old and now position
            SKPoint difference = _layerEditorService.GetScaledPoint(layer, _topLeft - path.Points[0], false);
            // Apply the difference so that the shape effectively stays in place
            layer.Transform.Position.SetCurrentValue(layer.Transform.Position.CurrentValue + difference, ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        #endregion

        #region Private methods

        private static SKPoint RoundPoint(SKPoint point, int decimals)
        {
            return new((float) Math.Round(point.X, decimals, MidpointRounding.AwayFromZero), (float) Math.Round(point.Y, decimals, MidpointRounding.AwayFromZero));
        }

        private static SKPoint[] UnTransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot, bool includeScale)
        {
            using SKPath counterRotatePath = new();
            counterRotatePath.AddPoly(skPoints, false);
            counterRotatePath.Transform(SKMatrix.CreateRotationDegrees(layer.Transform.Rotation.CurrentValue * -1, pivot.X, pivot.Y));
            if (includeScale)
                counterRotatePath.Transform(SKMatrix.CreateScale(1f / (layer.Transform.Scale.CurrentValue.Width / 100f), 1f / (layer.Transform.Scale.CurrentValue.Height / 100f)));

            return counterRotatePath.Points;
        }

        private static Point GetRelativePosition(object sender, MouseEventArgs mouseEventArgs)
        {
            DependencyObject parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return mouseEventArgs.GetPosition((IInputElement) parent);
        }

        private float CalculateAngle(Layer layer, object mouseEventSender, MouseEventArgs mouseEvent)
        {
            Point start = _layerEditorService.GetLayerAnchorPosition(layer);
            Point arrival = GetRelativePosition(mouseEventSender, mouseEvent);

            float radian = (float) Math.Atan2(start.Y - arrival.Y, start.X - arrival.X);
            float angle = radian * (180f / (float) Math.PI);
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