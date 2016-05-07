using Artemis.Managers;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Memory;
using Artemis.ViewModels.Abstract;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public sealed class RocketLeagueViewModel : GameViewModel<RocketLeagueDataModel>
    {
        private string _versionText;

        public RocketLeagueViewModel(MainManager mainManager, KeyboardManager keyboardManager,
            EffectManager effectManager)
            : base(
                mainManager, effectManager,
                new RocketLeagueModel(mainManager, keyboardManager, new RocketLeagueSettings()))
        {
            DisplayName = "Rocket League";

            EffectManager.EffectModels.Add(GameModel);
            SetVersionText();
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

        public RocketLeagueModel RocketLeagueModel { get; set; }

        private void SetVersionText()
        {
            if (!General.Default.EnablePointersUpdate)
            {
                VersionText = "Note: You disabled pointer updates, this could result in the " +
                              "Rocket League effect not working after a game update.";
                return;
            }

            Updater.GetPointers();
            var version = JsonConvert
                .DeserializeObject<GamePointersCollection>(Offsets.Default.RocketLeague)
                .GameVersion;
            VersionText = $"Note: Requires patch {version}. When a new patch is released Artemis downloads " +
                          "new pointers for the latest version (unless disabled in settings).";
        }
    }
}