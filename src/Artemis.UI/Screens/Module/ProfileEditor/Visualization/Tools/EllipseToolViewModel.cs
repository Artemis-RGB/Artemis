using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class EllipseToolViewModel : VisualizationToolViewModel
    {
        private bool _shiftDown;

        public EllipseToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            using (var stream = new MemoryStream(Resources.aero_crosshair))
            {
                Cursor = new Cursor(stream);
            }
        }

        public Rect DragRectangle { get; set; }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            if (!IsMouseDown)
                return;

            var position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (!_shiftDown)
                DragRectangle = new Rect(MouseDownStartPosition, position);
            else
                DragRectangle = GetSquareRectBetweenPoints(MouseDownStartPosition, position);
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                // Ensure the shape is an ellipse, we're all about ellipses up here
                if (!(layer.LayerShape is Ellipse))
                    layer.LayerShape = new Ellipse(layer);

                // Apply the drag rectangle
                layer.LayerShape.SetFromUnscaledRectangle(DragRectangle.ToSKRect());
                ProfileEditorService.UpdateSelectedProfileElement();
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            base.KeyUp(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftDown = false;
        }

        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftDown = true;
        }
    }
}