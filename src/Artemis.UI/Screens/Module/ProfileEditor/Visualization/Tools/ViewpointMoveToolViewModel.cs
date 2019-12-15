using System.Windows.Input;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class ViewpointMoveToolViewModel : VisualizationToolViewModel
    {
        public ViewpointMoveToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            Cursor = Cursors.Hand;
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            if (IsMouseDown)
                ProfileViewModel.PanZoomViewModel.ProcessMouseMove(sender, e);
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
            ProfileViewModel.PanZoomViewModel.ProcessMouseScroll(sender, e);
        }
    }
}