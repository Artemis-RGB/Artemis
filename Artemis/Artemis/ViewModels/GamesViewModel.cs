using Artemis.Models;
using Artemis.Modules.Games.CounterStrike;
using Artemis.Modules.Games.Dota2;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Modules.Games.Witcher3;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class GamesViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public GamesViewModel(MainModel mainModel)
        {
            ActivateItem(new RocketLeagueViewModel(mainModel) {DisplayName = "Rocket League"});
            ActivateItem(new CounterStrikeViewModel(mainModel) {DisplayName = "CS:GO"});
            ActivateItem(new Dota2ViewModel(mainModel) {DisplayName = "Dota 2 (NYI)"});
            ActivateItem(new Witcher3ViewModel(mainModel) {DisplayName = "The Witcher 3"});
        }
    }
}