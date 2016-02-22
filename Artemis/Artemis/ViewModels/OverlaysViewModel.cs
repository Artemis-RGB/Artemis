using Artemis.Models;
using Artemis.Modules.Overlays.VolumeDisplay;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class OverlaysViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly MainModel _mainModel;
        private VolumeDisplayViewModel _volumeDisplayVm;

        public OverlaysViewModel(MainModel mainModel)
        {
            _mainModel = mainModel;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Items.Clear();

            // This VM appears to be going out of scope, so recreating it every time just to be sure.
            _volumeDisplayVm = new VolumeDisplayViewModel(_mainModel) { DisplayName = "Volume Display" };
            ActivateItem(_volumeDisplayVm);
            ActiveItem = _volumeDisplayVm;
        }
    }
}