using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Artemis.DAL;
using Ninject;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.UnrealTournament
{
    public sealed class UnrealTournamentViewModel : GameViewModel
    {
        public UnrealTournamentViewModel(MainManager main, IKernel kernel, UnrealTournamentModel model)
            : base(main, model, kernel)
        {
            DisplayName = "Unreal Tournament";
            MainManager.EffectManager.EffectModels.Add(GameModel);
            FindGame();
            InstallGif();
        }

        public UnrealTournamentModel UnrealTournamentModel { get; set; }

        public void FindGame()
        {
            var gameSettings = (UnrealTournamentSettings) GameSettings;
            // If already propertly set up, don't do anything
            if ((gameSettings.GameDirectory != null) &&
                File.Exists(gameSettings.GameDirectory + "UE4-Win64-Shipping.exe"))
                return;

            // Attempt to read the file
            if (!File.Exists(@"C:\ProgramData\Epic\UnrealEngineLauncher\LauncherInstalled.dat"))
                return;

            var json =
                JsonConvert.DeserializeObject<JObject>(
                    File.ReadAllText(@"C:\ProgramData\Epic\UnrealEngineLauncher\LauncherInstalled.dat"));
            var utEntry =
                json["InstallationList"].Children()
                    .FirstOrDefault(c => c["AppName"].Value<string>() == "UnrealTournamentDev");
            if (utEntry == null)
                return;

            var utDir = utEntry["InstallLocation"].Value<string>();
            // Use backslash in path for consistency
            utDir = utDir.Replace('/', '\\');

            if (!File.Exists(utDir + @"\UE4-Win64-Shipping.exe"))
                return;

            gameSettings.GameDirectory = utDir;
            gameSettings.Save();
            PlaceFiles();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = ((UnrealTournamentSettings) GameSettings).GameDirectory
            };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((UnrealTournamentSettings) GameSettings).GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => GameSettings);
            GameSettings.Save();

            PlaceFiles();
        }

        public void PlaceFiles()
        {
            var path = ((UnrealTournamentSettings) GameSettings).GameDirectory;

            if (!File.Exists(path + @"\UE4-Win64-Shipping.exe"))
            {
                DialogService.ShowErrorMessageBox("Please select a valid Unreal Tournament directory\n\n" +
                                                  @"By default Unreal Tournament is in C:\Program Files\Epic Games\UnrealTournament");

                ((UnrealTournamentSettings) GameSettings).GameDirectory = string.Empty;
                NotifyOfPropertyChange(() => GameSettings);
                GameSettings.Save();

                MainManager.Logger.Warn("Failed to install Unreal Tournament plugin in '{0}' (path not found)", path);
                return;
            }

            // Load the ZIP from resources
            using (var stream = new MemoryStream(Resources.ut_plugin))
            {
                var archive = new ZipArchive(stream);

                try
                {
                    Directory.CreateDirectory(path + @"\UnrealTournament\Plugins\Artemis");
                    archive.ExtractToDirectory(path + @"\UnrealTournament\Plugins\Artemis", true);
                }
                catch (Exception e)
                {
                    MainManager.Logger.Error(e, "Failed to install Unreal Tournament plugin in '{0}'", path);
                    return;
                }
            }
            MainManager.Logger.Info("Installed Unreal Tournament plugin in '{0}'", path);
        }

        private void InstallGif()
        {
            var gif = Resources.redeemer;
            if (gif == null)
                return;

            ProfileProvider.InsertGif("UnrealTournament", "Default", "Redeemer GIF", gif, "redeemer");
        }
    }
}