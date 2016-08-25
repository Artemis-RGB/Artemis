using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Effects.Bubbles
{
    public sealed class BubblesViewModel : EffectViewModel
    {
        public BubblesViewModel(MainManager main, BubblesModel model) : base(main, model)
        {
            DisplayName = "Bubbles";
        }
    }
}