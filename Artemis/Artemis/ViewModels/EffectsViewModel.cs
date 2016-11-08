using System.Linq;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class EffectsViewModel : BaseViewModel
    {
        private IOrderedEnumerable<EffectViewModel> _vms;

        public EffectsViewModel(EffectViewModel[] effectViewModels)
        {
            DisplayName = "Effects";

            _vms = effectViewModels.OrderBy(o => o.DisplayName);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}