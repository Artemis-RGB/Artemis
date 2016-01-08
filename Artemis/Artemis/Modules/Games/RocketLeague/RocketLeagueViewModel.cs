using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueViewModel : Screen
    {
        private RocketLeagueSettings _rocketLeagueSettings;

        public RocketLeagueViewModel(MainModel mainModel)
        {
            MainModel = mainModel;

            // Settings are loaded from file by class
            RocketLeagueSettings = new RocketLeagueSettings();

            // Create effect model and add it to MainModel
            RocketLeagueModel = new RocketLeagueModel(RocketLeagueSettings);
            MainModel.EffectModels.Add(RocketLeagueModel);
        }

        public static string Name => "Rocket League";

        public MainModel MainModel { get; set; }
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
    }
}