using System.IO;
using System.Windows.Forms;
using Artemis.Models;
using Artemis.Properties;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeViewModel : Screen
    {
        private CounterStrikeSettings _counterStrikeSettings;

        public CounterStrikeViewModel(MainModel mainModel)
        {
            MainModel = mainModel;

            // Settings are loaded from file by class
            CounterStrikeSettings = new CounterStrikeSettings();

            // Create effect model and add it to MainModel
            CounterStrikeModel = new CounterStrikeModel(mainModel, CounterStrikeSettings);
            MainModel.EffectModels.Add(CounterStrikeModel);
            PlaceConfigFile();
        }

        public CounterStrikeSettings CounterStrikeSettings
        {
            get { return _counterStrikeSettings; }
            set
            {
                if (Equals(value, _counterStrikeSettings)) return;
                _counterStrikeSettings = value;
                NotifyOfPropertyChange(() => CounterStrikeSettings);
            }
        }

        public CounterStrikeModel CounterStrikeModel { get; set; }

        public MainModel MainModel { get; set; }

        public static string Name => "CS:GO";
        public string Content => "Counter-Strike: GO Content";

        public void SaveSettings()
        {
            CounterStrikeSettings?.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            CounterStrikeSettings.ToDefault();
            NotifyOfPropertyChange(() => CounterStrikeSettings);

            SaveSettings();
        }

        public void ToggleEffect()
        {
            CounterStrikeModel.Enabled = _counterStrikeSettings.Enabled;
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = CounterStrikeSettings.GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            CounterStrikeSettings.GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => CounterStrikeSettings);
            PlaceConfigFile();
        }

        public void PlaceConfigFile()
        {
            if (CounterStrikeSettings.GameDirectory == string.Empty)
                return;
            if (Directory.Exists(CounterStrikeSettings.GameDirectory + "/csgo/cfg"))
            {
                var cfgFile = Resources.gamestateConfigFileCsGo.Replace("{{port}}",
                    MainModel.GameStateWebServer.Port.ToString());
                File.WriteAllText(CounterStrikeSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg",
                    cfgFile);
                return;
            }

            MessageBox.Show("Please select a valid CS:GO directory");
            CounterStrikeSettings.GameDirectory = string.Empty;
            NotifyOfPropertyChange(() => CounterStrikeSettings);
        }
    }
}