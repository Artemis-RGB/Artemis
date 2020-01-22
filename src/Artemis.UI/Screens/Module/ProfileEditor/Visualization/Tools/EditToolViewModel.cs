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
        private Point _dragStart;
        private SKPoint _dragStartAnchor;
        private bool _isDragging;

        public EditToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
            : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            Cursor = Cursors.Arrow;
            Update();

            profileEditorService.SelectedProfileChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            profileEditorService.ProfilePreviewUpdated += (sender, args) => Update();
        }

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
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer) || layer.LayerShape == null) 
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
            });
        }

        public void ShapeEditMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
                return;

            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                // The path starts at 0,0 so there's no simple way to get the position relative to the top-left of the path
                var dragStartPosition = GetRelativePosition(sender, e);
                var anchor = _layerEditorService.GetLayerAnchor(layer, true);

//                _dragOffset = new Point(dragStartPosition.X - anchor.X - 1.45, dragStartPosition.Y - anchor.Y - 1.45);
                _dragStart = dragStartPosition;
            }

            _isDragging = true;
            _draggingHorizontally = false;
            _draggingVertically = false;

            ((IInputElement) sender).CaptureMouse();
            e.Handled = true;
        }

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
            _draggingHorizontally = false;
            _draggingVertically = false;

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

        public void AnchorMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;
            
            // The start anchor is relative to an unrotated version of the shape
            var start = _dragStartAnchor;
            // Add the current position to the start anchor to determine the new position
            var current = start + (GetRelativePosition(sender, e).ToSKPoint() - _dragOffset);
            // In order to keep the mouse movement unrotated, counter-act the active rotation
            var countered = UnTransformPoints(new[] {start, current}, layer, start);
            var scaled = _layerEditorService.GetScaledPoint(layer, countered[1], false);

            // Update the anchor point, this causes the shape to move
            layer.AnchorPointProperty.SetCurrentValue(scaled, ProfileEditorService.CurrentTime);
            // TopLeft is not updated yet and acts as a snapshot of the top-left before changing the anchor
            var path = _layerEditorService.GetLayerPath(layer, true, true, true);
            // Calculate the (scaled) difference between the old and now position
            var difference = _layerEditorService.GetScaledPoint(layer, TopLeft - path.Points[0], false);
            // Apply the difference so that the shape effectively stays in place
            layer.PositionProperty.SetCurrentValue(layer.PositionProperty.CurrentValue + difference, ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void Move(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);
            var x = (float) (position.X - _dragOffset.X);
            var y = (float) (position.Y - _dragOffset.Y);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (_draggingVertically)
                    x = (float) (_dragStart.X - _dragOffset.X);
                else if (_draggingHorizontally)
                    y = (float) (_dragStart.Y - _dragOffset.Y);
                else
                {
                    _draggingHorizontally = Math.Abs(position.X - _dragStart.X) > Math.Abs(position.Y - _dragStart.Y);
                    _draggingVertically = Math.Abs(position.X - _dragStart.X) < Math.Abs(position.Y - _dragStart.Y);
                    return;
                }
            }

            var scaled = _layerEditorService.GetScaledPoint(layer, new SKPoint(x, y), true);
            layer.PositionProperty.SetCurrentValue(scaled, ProfileEditorService.CurrentTime);

            ProfileEditorService.UpdateProfilePreview();
        }

        public void TopLeftRotate(object sender, MouseEventArgs e)
        {
        }

        public void TopLeftResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);
            var skRect = layer.LayerShape.RenderRectangle;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Take the greatest difference
                // Base the smallest difference on the greatest difference, maintaining aspect ratio
            }
            else
            {
                skRect.Top = (float) Math.Min(position.Y, skRect.Bottom);
                skRect.Left = (float) Math.Min(position.X, skRect.Bottom);
            }

            ApplyShapeResize(skRect);
        }

        public void TopCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.RenderRectangle;
            skRect.Top = (float) Math.Min(position.Y, skRect.Bottom);
            ApplyShapeResize(skRect);
        }

        public void TopRightRotate(object sender, MouseEventArgs e)
        {
        }

        public void TopRightResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);
            var skRect = layer.LayerShape.RenderRectangle;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Take the greatest difference
                // Base the smallest difference on the greatest difference, maintaining aspect ratio
            }
            else
            {
                skRect.Top = (float) Math.Min(position.Y, skRect.Bottom);
                skRect.Right = (float) Math.Max(position.X, skRect.Left);
            }

            ApplyShapeResize(skRect);
        }

        public void CenterRightResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.RenderRectangle;
            skRect.Right = (float) Math.Max(position.X, skRect.Left);
            ApplyShapeResize(skRect);
        }

        public void BottomRightRotate(object sender, MouseEventArgs e)
        {
        }

        public void BottomRightResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);
            var skRect = layer.LayerShape.RenderRectangle;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Take the greatest difference
                // Base the smallest difference on the greatest difference, maintaining aspect ratio
            }
            else
            {
                skRect.Bottom = (float) Math.Max(position.Y, skRect.Top);
                skRect.Right = (float) Math.Max(position.X, skRect.Left);
            }

            ApplyShapeResize(skRect);
        }

        public void BottomCenterResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.RenderRectangle;
            skRect.Bottom = (float) Math.Max(position.Y, skRect.Top);
            ApplyShapeResize(skRect);
        }

        public void BottomLeftRotate(object sender, MouseEventArgs e)
        {
        }

        public void BottomLeftResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);
            var skRect = layer.LayerShape.RenderRectangle;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Take the greatest difference
                // Base the smallest difference on the greatest difference, maintaining aspect ratio
            }
            else
            {
                skRect.Bottom = (float) Math.Max(position.Y, skRect.Top);
                skRect.Left = (float) Math.Min(position.X, skRect.Right);
            }

            ApplyShapeResize(skRect);
        }

        public void CenterLeftResize(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.RenderRectangle;
            skRect.Left = (float) Math.Min(position.X, skRect.Right);
            ApplyShapeResize(skRect);
        }

        private SKPoint[] UnTransformPoints(SKPoint[] skPoints, Layer layer, SKPoint pivot)
        {
            var counterRotatePath = new SKPath();
            counterRotatePath.AddPoly(skPoints, false);
            counterRotatePath.Transform(SKMatrix.MakeRotationDegrees(layer.RotationProperty.CurrentValue * -1, pivot.X, pivot.Y));
            counterRotatePath.Transform(SKMatrix.MakeScale(1f / layer.SizeProperty.CurrentValue.Width, 1f / layer.SizeProperty.CurrentValue.Height));

            return counterRotatePath.Points;
        }

        private Point GetRelativePosition(object sender, MouseEventArgs mouseEventArgs)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return mouseEventArgs.GetPosition((IInputElement) parent);
        }

        private void ApplyShapeResize(SKRect newRect)
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            // TODO: Apply the translation
            // Store the original position to create an offset for the anchor
            // var original = layer.PositionProperty.CurrentValue;
            // layer.LayerShape.SetFromUnscaledRectangle(newRect, ProfileEditorService.CurrentTime);
            // var updated = layer.PositionProperty.CurrentValue;
            // // Apply the offset to the anchor so it stays in at same spot
            // layer.AnchorPointProperty.SetCurrentValue(new SKPoint(
            //     layer.AnchorPointProperty.CurrentValue.X + (original.X - updated.X),
            //     layer.AnchorPointProperty.CurrentValue.Y + (original.Y - updated.Y)
            // ), ProfileEditorService.CurrentTime);

            // Update the preview
            ProfileEditorService.UpdateProfilePreview();
        }
    }
}