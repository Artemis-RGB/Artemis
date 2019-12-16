using System.IO;
using System.Windows;
using System.Windows.Input;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;

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