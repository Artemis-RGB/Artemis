using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.CounterStrike
{
    public sealed class CounterStrikeViewModel : GameViewModel
    {
        public CounterStrikeViewModel(MainManager main, IKernel kernel, [Named("CounterStrikeModel")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "CS:GO";
        }

        public void BrowseDirectory()
        {
            ((CounterStrikeModel) GameModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => GameSettings);
        }
    }
}