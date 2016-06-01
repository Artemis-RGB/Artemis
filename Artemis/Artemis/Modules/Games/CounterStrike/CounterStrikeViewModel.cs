using System.IO;
using System.Windows.Forms;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Games.CounterStrike
{
    public sealed class CounterStrikeViewModel : GameViewModel
    {
        public CounterStrikeViewModel(MainManager main, IEventAggregator events, IProfileEditorViewModelFactory pFactory)
            : base(main, new CounterStrikeModel(main, new CounterStrikeSettings()), events, pFactory)
        {
            DisplayName = "CS:GO";
            MainManager.EffectManager.EffectModels.Add(GameModel);

            FindGameDir();
            PlaceConfigFile();
        }

        public void FindGameDir()
        {
            var gameSettings = (CounterStrikeSettings) GameSettings;
            // If already propertly set up, don't do anything
            if (gameSettings.GameDirectory != null && File.Exists(gameSettings.GameDirectory + "csgo.exe") &&
                File.Exists(gameSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg"))
                return;

            var dir = GeneralHelpers.FindSteamGame(@"\Counter-Strike Global Offensive\csgo.exe");
            gameSettings.GameDirectory = dir ?? string.Empty;
            gameSettings.Save();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((CounterStrikeSettings) GameSettings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((CounterStrikeSettings) GameSettings).GameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
            PlaceConfigFile();
        }

        public void PlaceConfigFile()
        {
            if (((CounterStrikeSettings) GameSettings).GameDirectory == string.Empty)
                return;

            var path = ((CounterStrikeSettings) GameSettings).GameDirectory;
            if (Directory.Exists(path + "/csgo/cfg"))
            {
                var cfgFile = Resources.csgoGamestateConfiguration.Replace("{{port}}",
                    MainManager.GameStateWebServer.Port.ToString());
                File.WriteAllText(path + "/csgo/cfg/gamestate_integration_artemis.cfg", cfgFile);

                return;
            }

            DialogService.ShowErrorMessageBox("Please select a valid CS:GO directory\n\n" +
                                              @"By default CS:GO is in \SteamApps\common\Counter-Strike Global Offensive");

            ((CounterStrikeSettings) GameSettings).GameDirectory = string.Empty;
            NotifyOfPropertyChange(() => GameSettings);
            GameSettings.Save();
        }
    }
}