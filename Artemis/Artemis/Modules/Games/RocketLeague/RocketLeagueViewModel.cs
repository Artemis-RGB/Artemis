using Artemis.Managers;
using Caliburn.Micro;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueViewModel : Screen
    {
        private RocketLeagueSettings _rocketLeagueSettings;

        public RocketLeagueViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            RocketLeagueSettings = new RocketLeagueSettings();

            // Create effect model and add it to MainManager
            RocketLeagueModel = new RocketLeagueModel(mainManager, RocketLeagueSettings);
            MainManager.EffectManager.EffectModels.Add(RocketLeagueModel);
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