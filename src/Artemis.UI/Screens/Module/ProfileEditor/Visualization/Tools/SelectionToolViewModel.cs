using System.IO;
using System.Windows;
using System.Windows.Input;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class SelectionToolViewModel : VisualizationToolViewModel
    {
        public SelectionToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            using (var stream = new MemoryStream(Resources.aero_crosshair))
            {
                Cursor = new Cursor(stream);
            }
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.MouseDown(sender, e);
//            ProfileViewModel.SelectionRectangle.Rect = new Rect();
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(MouseDownStartPosition, position);

            foreach (var device in ProfileViewModel.Devices)
            {
                foreach (var ledViewModel in device.Leds)
                {
                    if (ProfileViewModel.PanZoomViewModel.TransformContainingRect(ledViewModel.Led.RgbLed.AbsoluteLedRectangle).IntersectsWith(selectedRect))
                        ledViewModel.IsSelected = true;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        ledViewModel.IsSelected = false;
                }
            }
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(MouseDownStartPosition, position);
//            ProfileViewModel.SelectionRectangle.Rect = selectedRect;

            foreach (var device in ProfileViewModel.Devices)
            {
                foreach (var ledViewModel in device.Leds)
                {
                    if (ProfileViewModel.PanZoomViewModel.TransformContainingRect(ledViewModel.Led.RgbLed.AbsoluteLedRectangle).IntersectsWith(selectedRect))
                        ledViewModel.IsSelected = true;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        ledViewModel.IsSelected = false;
                }
            }
        }
    }
}