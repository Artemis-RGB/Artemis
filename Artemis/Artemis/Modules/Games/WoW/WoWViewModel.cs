using Ninject;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.WoW
{
    public sealed class WoWViewModel : GameViewModel
    {
        public WoWViewModel(MainManager main, IKernel kernel, WoWModel model)
            : base(main, model, kernel)
        {
            DisplayName = "WoW";
        }
    }
}