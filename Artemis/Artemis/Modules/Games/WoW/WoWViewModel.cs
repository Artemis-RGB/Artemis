using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.WoW
{
    public sealed class WoWViewModel : GameViewModel
    {
        public WoWViewModel(MainManager main, IProfileEditorVmFactory pFactory, WoWModel model)
            : base(main, model, pFactory)
        {
            DisplayName = "WoW";
        }
    }
}