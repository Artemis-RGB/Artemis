using System.Linq;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class OverlaysViewModel : BaseViewModel
    {
        private IOrderedEnumerable<OverlayViewModel> _vms;

        public OverlaysViewModel(OverlayViewModel[] overlayViewModels)
        {
            DisplayName = "Overlays";
            _vms = overlayViewModels.OrderBy(o => o.DisplayName);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}