using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Games.UnrealTournament
{
    public sealed class UnrealTournamentViewModel : GameViewModel
    {
        private string _versionText;

        public UnrealTournamentViewModel(MainManager main, IEventAggregator events, IProfileEditorVmFactory pFactory)
            : base(main, new UnrealTournamentModel(main, new UnrealTournamentSettings()), events, pFactory)
        {
            DisplayName = "Unreal Tournament";

            MainManager.EffectManager.EffectModels.Add(GameModel);
        }

        public string VersionText
        {
            get { return _versionText; }
            set
            {
                if (value == _versionText) return;
                _versionText = value;
                NotifyOfPropertyChange(() => VersionText);
            }
        }

        public UnrealTournamentModel UnrealTournamentModel { get; set; }
    }
}