using System.Windows.Input;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Visualization.Tools
{
    public class ViewpointMoveToolViewModel : VisualizationToolViewModel
    {
        public ViewpointMoveToolViewModel(PanZoomViewModel panZoomViewModel, IProfileEditorService profileEditorService) : base(panZoomViewModel, profileEditorService)
        {
            Cursor = Cursors.Hand;
            PanZoomViewModel.LastPanPosition = null;
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            PanZoomViewModel.ProcessMouseMove(sender, e);
        }
    }
}