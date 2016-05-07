using Artemis.Managers;
using Artemis.Modules.Overlays.VolumeDisplay;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class OverlaysViewModel : BaseViewModel
    {
        private readonly MainManager _mainManager;
        private readonly OverlayViewModel[] _overlayViewModels;
        private VolumeDisplayViewModel _volumeDisplayVm;

        public OverlaysViewModel(MainManager mainManager, OverlayViewModel[] overlayViewModels)
        {
            DisplayName = "Overlays";

            _mainManager = mainManager;
            _overlayViewModels = overlayViewModels;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            foreach (var overlayViewModel in _overlayViewModels)
                ActivateItem(overlayViewModel);
        }
    }
}