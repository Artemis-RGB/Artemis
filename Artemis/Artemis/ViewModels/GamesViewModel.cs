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
        private readonly RocketLeagueViewModel _rocketLeagueVm;
        private readonly CounterStrikeViewModel _counterStrikeVm;
        private readonly Dota2ViewModel _dota2Vm;
        private readonly Witcher3ViewModel _witcher3Vm;

        public GamesViewModel(MainModel mainModel)
        {
            _rocketLeagueVm = new RocketLeagueViewModel(mainModel) {DisplayName = "Rocket League"};
            _counterStrikeVm = new CounterStrikeViewModel(mainModel) { DisplayName = "CS:GO" };
            _dota2Vm = new Dota2ViewModel(mainModel) { DisplayName = "Dota 2 (NYI)" };
            _witcher3Vm = new Witcher3ViewModel(mainModel) { DisplayName = "The Witcher 3" };
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_rocketLeagueVm);
            ActivateItem(_counterStrikeVm);
            ActivateItem(_dota2Vm);
            ActivateItem(_witcher3Vm);
        }
    }
}