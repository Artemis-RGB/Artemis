using System.Windows.Input;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class ViewpointMoveToolViewModel : VisualizationToolViewModel
    {
        public ViewpointMoveToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            Cursor = Cursors.Hand;
            ProfileViewModel.PanZoomViewModel.LastPanPosition = null;
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            ProfileViewModel.PanZoomViewModel.ProcessMouseMove(sender, e);
        }
    }
}