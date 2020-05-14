using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineViewModel
    {
        public TimelineViewModel(BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups)
        {
            LayerPropertyGroups = layerPropertyGroups;
        }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }
    }
}