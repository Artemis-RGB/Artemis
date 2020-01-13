using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class EditToolViewModel : VisualizationToolViewModel
    {
        private bool _mouseDown;

        public EditToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            Cursor = Cursors.Arrow;
            Update();

            profileEditorService.SelectedProfileChanged += (sender, args) => Update();
            profileEditorService.SelectedProfileElementUpdated += (sender, args) => Update();
            profileEditorService.CurrentTimeChanged += (sender, args) => Update();
        }

        private void Update()
        {
            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                ShapeSkRect = layer.LayerShape.GetUnscaledRectangle();
            }
        }

        public SKRect ShapeSkRect { get; set; }

        public void ShapeEditMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            _mouseDown = true;
            e.Handled = true;
        }

        public void ShapeEditMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
            _mouseDown = false;
            e.Handled = true;
        }

        public void TopLeftRotate(object sender, MouseEventArgs e)
        {
        }

        public void TopLeftResize(object sender, MouseEventArgs e)
        {
            
        }

        public void TopCenterResize(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
                return;
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.GetUnscaledRectangle();
            skRect.Top = (float) Math.Min(position.Y, skRect.Bottom);
            layer.LayerShape.SetFromUnscaledRectangle(skRect);

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public void TopRightRotate(object sender, MouseEventArgs e)
        {
        }

        public void TopRightResize(object sender, MouseEventArgs e)
        {
        }

        public void CenterRightResize(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
                return;
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.GetUnscaledRectangle();
            skRect.Right = (float) Math.Max(position.X, skRect.Left);
            layer.LayerShape.SetFromUnscaledRectangle(skRect);

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        private Point GetRelativePosition(object sender, MouseEventArgs mouseEventArgs)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject) sender);
            return mouseEventArgs.GetPosition((IInputElement) parent);
        }

        public void BottomRightRotate(object sender, MouseEventArgs e)
        {
        }

        public void BottomRightResize(object sender, MouseEventArgs e)
        {
        }

        public void BottomCenterResize(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
                return;
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.GetUnscaledRectangle();
            skRect.Bottom = (float) Math.Max(position.Y, skRect.Top);
            layer.LayerShape.SetFromUnscaledRectangle(skRect);

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public void BottomLeftRotate(object sender, MouseEventArgs e)
        {
        }

        public void BottomLeftResize(object sender, MouseEventArgs e)
        {
        }

        public void CenterLeftResize(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
                return;
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
                return;

            var position = GetRelativePosition(sender, e);

            var skRect = layer.LayerShape.GetUnscaledRectangle();
            skRect.Left = (float) Math.Min(position.X, skRect.Right);
            layer.LayerShape.SetFromUnscaledRectangle(skRect);

            ProfileEditorService.UpdateSelectedProfileElement();
        }
    }
}