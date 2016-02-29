using Artemis.Managers;
using Artemis.Models;
using Artemis.Settings;
using Artemis.Utilities;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueViewModel : Screen
    {
        private RocketLeagueSettings _rocketLeagueSettings;
        private string _versionText;

        public RocketLeagueViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            RocketLeagueSettings = new RocketLeagueSettings();

            // Create effect model and add it to MainManager
            RocketLeagueModel = new RocketLeagueModel(mainManager, RocketLeagueSettings);
            MainManager.EffectManager.EffectModels.Add(RocketLeagueModel);

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

        public static string Name => "Rocket League";

        public MainManager MainManager { get; set; }
        public RocketLeagueModel RocketLeagueModel { get; set; }

        public RocketLeagueSettings RocketLeagueSettings
        {
            get { return _rocketLeagueSettings; }
            set
            {
                if (Equals(value, _rocketLeagueSettings)) return;
                _rocketLeagueSettings = value;
                NotifyOfPropertyChange(() => RocketLeagueSettings);
            }
        }

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
                .DeserializeObject<GamePointersCollectionModel>(Offsets.Default.RocketLeague)
                .GameVersion;
            VersionText = $"Note: Requires patch {version}. When a new patch is released Artemis downloads " +
                          "new pointers for the latest version (unless disabled in settings).";
        }

        public void SaveSettings()
        {
            if (RocketLeagueModel == null)
                return;

            RocketLeagueSettings.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            RocketLeagueSettings.ToDefault();
            NotifyOfPropertyChange(() => RocketLeagueSettings);

            SaveSettings();
        }

        public void ToggleEffect()
        {
            RocketLeagueModel.Enabled = _rocketLeagueSettings.Enabled;
        }
    }
}