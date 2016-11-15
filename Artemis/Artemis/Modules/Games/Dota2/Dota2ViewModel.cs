using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.Dota2
{
    public sealed class Dota2ViewModel : GameViewModel
    {
        public Dota2ViewModel(MainManager main, IKernel kernel, [Named("Dota2Model")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "Dota 2";
        }

        public void PlaceConfigFile()
        {
            ((Dota2Model) GameModel).PlaceConfigFile();
            NotifyOfPropertyChange(() => GameSettings);
        }
    }
}