using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyGroupViewModel
    {
        public TimelinePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel) layerPropertyBaseViewModel;
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
    }
}