using System.IO;
using System.Windows.Forms;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public sealed class EurotruckSimulator2ViewModel : GameViewModel
    {
        public EurotruckSimulator2ViewModel(MainManager main, IProfileEditorVmFactory pFactory,
            EurotruckSimulator2Model model) : base(main, model, pFactory)
        {
            DisplayName = "ETS 2";

            FindGameDir();
            PlacePlugin();
        }

        public void FindGameDir()
        {
            var gameSettings = (EurotruckSimulator2Settings) GameSettings;
            // If already propertly set up, don't do anything
            //if (gameSettings.GameDirectory != null && File.Exists(gameSettings.GameDirectory + "csgo.exe") &&
            //    File.Exists(gameSettings.GameDirectory + "/csgo/cfg/gamestate_integration_artemis.cfg"))
            //    return;

            // Demo is also supported but resides in a different directory
            var dir = GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x86\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2 Demo\bin\win_x86\eurotrucks2.exe");

            gameSettings.GameDirectory = dir ?? string.Empty;
            gameSettings.Save();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = ((EurotruckSimulator2Settings) GameSettings).GameDirectory
            };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((EurotruckSimulator2Settings) GameSettings).GameDirectory = Path.GetDirectoryName(dialog.SelectedPath);
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
            PlacePlugin();
        }

        public void PlacePlugin()
        {
            if (((EurotruckSimulator2Settings) GameSettings).GameDirectory == string.Empty)
                return;

            var path = ((EurotruckSimulator2Settings) GameSettings).GameDirectory;
            //if (Directory.Exists(path + "/csgo/cfg"))
            //{
            //    var cfgFile = Resources.csgoGamestateConfiguration.Replace("{{port}}",
            //        MainManager.GameStateWebServer.Port.ToString());
            //    File.WriteAllText(path + "/csgo/cfg/gamestate_integration_artemis.cfg", cfgFile);

            //    return;
            //}

            //DialogService.ShowErrorMessageBox("Please select a valid CS:GO directory\n\n" +
            //                                  @"By default CS:GO is in \SteamApps\common\Counter-Strike Global Offensive");

            //((EurotruckSimulator2Settings) GameSettings).GameDirectory = string.Empty;
            //NotifyOfPropertyChange(() => GameSettings);
            //GameSettings.Save();
        }
    }
}