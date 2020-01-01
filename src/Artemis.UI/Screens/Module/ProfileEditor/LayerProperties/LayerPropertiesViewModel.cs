using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        public LayerPropertiesViewModel()
        {
            Timeline = new LayerPropertiesTimelineViewModel();
        }

        public LayerPropertiesTimelineViewModel Timeline { get; set; }
    }
}