using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class OverlaysViewModel : BaseViewModel
    {
        private readonly OverlayViewModel[] _overlayViewModels;

        public OverlaysViewModel(OverlayViewModel[] overlayViewModels)
        {
            DisplayName = "Overlays";
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