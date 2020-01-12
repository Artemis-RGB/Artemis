using System.IO;
using System.Windows;
using System.Windows.Input;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class SelectionRemoveToolViewModel : VisualizationToolViewModel
    {
        public SelectionRemoveToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            using (var stream = new MemoryStream(Resources.aero_pen_min))
            {
                Cursor = new Cursor(stream);
            }
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(MouseDownStartPosition, position);

            foreach (var device in ProfileViewModel.DeviceViewModels)
            {
                foreach (var ledViewModel in device.Leds)
                {
                    if (ProfileViewModel.PanZoomViewModel.TransformContainingRect(ledViewModel.Led.RgbLed.AbsoluteLedRectangle).IntersectsWith(selectedRect))
                        ledViewModel.IsSelected = false;
                }
            }
        }
    }
}