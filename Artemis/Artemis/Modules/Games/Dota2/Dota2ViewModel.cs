using System;
using System.IO;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2ViewModel : GameViewModel
    {
        public Dota2ViewModel(MainManager mainManager)
        {
            MainManager = mainManager;
            GameSettings = new Dota2Settings();
            
            GameModel = new Dota2Model(mainManager, (Dota2Settings) GameSettings);
            MainManager.EffectManager.EffectModels.Add(GameModel);
            PlaceConfigFile();
        }

        public static string Name => "Dota 2";
        public string Content => "Dota 2 Content";

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog { SelectedPath = ((Dota2Settings)GameSettings).GameDirectory };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((Dota2Settings)GameSettings).GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
            PlaceConfigFile();
        }

        public void PlaceConfigFile()
        {
            if (((Dota2Settings)GameSettings).GameDirectory == string.Empty)
                return;
            if (Directory.Exists(((Dota2Settings)GameSettings).GameDirectory + "/game/dota/cfg"))
            {
                var cfgFile = Resources.dotaGamestateConfiguration.Replace("{{port}}",
                    MainManager.GameStateWebServer.Port.ToString());
                try
                {
                    File.WriteAllText(
                    ((Dota2Settings)GameSettings).GameDirectory + "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg",
                    cfgFile);
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(((Dota2Settings) GameSettings).GameDirectory + "/game/dota/cfg/gamestate_integration/");
                    File.WriteAllText(
                    ((Dota2Settings)GameSettings).GameDirectory + "/game/dota/cfg/gamestate_integration/gamestate_integration_artemis.cfg",
                    cfgFile);
                }
                

                return;
            }

            MainManager.DialogService.ShowErrorMessageBox("Please select a valid Dota 2 directory\n\n" +
                                                          @"By default Dota 2 is in \SteamApps\common\dota 2 beta");
            ((Dota2Settings)GameSettings).GameDirectory = string.Empty;
            NotifyOfPropertyChange(() => GameSettings);

            GameSettings.Save();
        }
    }
}