using Artemis.Models;
using Artemis.Modules.Overlays.VolumeDisplay;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class OverlaysViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public OverlaysViewModel(MainModel mainModel)
        {
            ActivateItem(new VolumeDisplayViewModel(mainModel) {DisplayName = "Volume Display"});
        }
    }
}