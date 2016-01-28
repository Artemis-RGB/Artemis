using Artemis.Models;
using Artemis.Modules.Games.RocketLeague;
using Caliburn.Micro;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3ViewModel : Screen
    {
        private RocketLeagueSettings _rocketLeagueSettings;

        public Witcher3ViewModel(MainModel mainModel)
        {
            MainModel = mainModel;

            // Settings are loaded from file by class
            RocketLeagueSettings = new RocketLeagueSettings();

            // Create effect model and add it to MainModel
            Witcher3Model = new Witcher3Model(mainModel, RocketLeagueSettings);
            MainModel.EffectModels.Add(Witcher3Model);
        }

        public static string Name => "The Witcher 3";

        public MainModel MainModel { get; set; }
        public Witcher3Model Witcher3Model { get; set; }

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
            if (Witcher3Model == null)
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