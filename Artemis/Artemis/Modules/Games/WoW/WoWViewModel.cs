using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.WoW
{
    public sealed class WoWViewModel : GameViewModel
    {
        public WoWViewModel(MainManager main, IKernel kernel, [Named("WoWModel")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "WoW";
        }
    }
}