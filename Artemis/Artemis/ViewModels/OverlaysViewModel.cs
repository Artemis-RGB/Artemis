using Artemis.Models;
using Artemis.Modules.Overlays.VolumeDisplay;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class OverlaysViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly VolumeDisplayViewModel _volumeDisplayVm;

        public OverlaysViewModel(MainModel mainModel)
        {
            _volumeDisplayVm = new VolumeDisplayViewModel(mainModel) {DisplayName = "Volume Display"};
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_volumeDisplayVm);
        }
    }
}