using System.IO;
using System.Windows.Forms;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.Dota2
{
    public sealed class Dota2ViewModel : GameViewModel
    {
        public Dota2ViewModel(MainManager main, IProfileEditorVmFactory pFactory, Dota2Model model) : base(main, model, pFactory)
        {
            DisplayName = "Dota 2";

            FindGameDir();
            PlaceConfigFile();
        }

        public void FindGameDir()
        {
            var gameSettings = (Dota2Settings) GameSettings;
            // If already propertly set up, don't do anything
            if (gameSettings.GameDirectory != null && File.Exists(gameSettings.GameDirectory + "csgo.exe") &&
                File.Exists(gameSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg"))
                return;

            var dir = GeneralHelpers.FindSteamGame(@"\dota 2 beta\game\bin\win32\dota2.exe");
            // Remove subdirectories where they stuck the executable
            dir = dir?.Substring(0, dir.Length - 15);

            gameSettings.GameDirectory = dir ?? string.Empty;
            gameSettings.Save();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((Dota2Settings) GameSettings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((Dota2Settings) GameSettings).GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
            PlaceConfigFile();
        }

        public void PlaceConfigFile()
        {
            if (((Dota2Settings) GameSettings).GameDirectory == string.Empty)
                return;
            if (Directory.Exists(((Dota2Settings) GameSettings).GameDirectory + "/game/dota/cfg"))
            {
                var cfgFile = Resources.dotaGamestateConfiguration.Replace("{{port}}",
                    MainManager.GameStateWebServer.Port.ToString());
                try
                {
                    File.WriteAllText(
                        ((Dota2Settings) GameSettings).GameDirectory +
                        "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg",
                        cfgFile);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(((Dota2Settings) GameSettings).GameDirectory +
                                              "/game/dota/cfg/gamestate_integration/");
                    File.WriteAllText(
                        ((Dota2Settings) GameSettings).GameDirectory +
                        "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg",
                        cfgFile);
                }

                return;
            }

            DialogService.ShowErrorMessageBox("Please select a valid Dota 2 directory\n\n" +
                                              @"By default Dota 2 is in \SteamApps\common\dota 2 beta");
            ((Dota2Settings) GameSettings).GameDirectory = string.Empty;
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
        }
    }
}