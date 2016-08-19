using System.Linq;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class EffectsViewModel : BaseViewModel
    {
        private readonly EffectViewModel[] _effectViewModels;

        public EffectsViewModel(EffectViewModel[] effectViewModels)
        {
            DisplayName = "Effects";
            _effectViewModels = effectViewModels;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            foreach (var effectViewModel in _effectViewModels.OrderBy(e => e.DisplayName))
                ActivateItem(effectViewModel);
        }
    }
}