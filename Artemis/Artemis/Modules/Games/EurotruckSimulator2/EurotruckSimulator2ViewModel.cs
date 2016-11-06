using System;
using System.IO;
using System.Windows.Forms;
using Ninject;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public sealed class EurotruckSimulator2ViewModel : GameViewModel
    {
        public EurotruckSimulator2ViewModel(MainManager main, IKernel kernel,
            EurotruckSimulator2Model model) : base(main, model, kernel)
        {
            DisplayName = "ETS 2";

            FindGameDir();
        }

        public void FindGameDir()
        {
            var gameSettings = (EurotruckSimulator2Settings) GameSettings;
            // Demo is also supported but resides in a different directory, the full game can also be 64-bits
            var dir = GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x64\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x86\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2 Demo\bin\win_x86\eurotrucks2.exe");

            if (string.IsNullOrEmpty(dir))
                return;

            gameSettings.GameDirectory = dir;
            gameSettings.Save();

            if (!File.Exists(dir + "/plugins/ets2-telemetry-server.dll"))   
                PlacePlugin();
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

            // Ensure the selected directory exists
            if (!Directory.Exists(path))
            {
                DialogService.ShowErrorMessageBox($"Directory '{path}' not found.");
                return;
            }
            // Ensure it's the ETS2 directory by looking for the executable
            if (!File.Exists(path + "/eurotrucks2.exe"))
            {
                DialogService.ShowErrorMessageBox("Please select a valid Eurotruck Simulator 2 directory\n\n" +
                                                  @"By default ETS2 is in \SteamApps\common\Euro Truck Simulator 2\bin\win_x64");
                return;
            }

            // Create the plugins folder if it's not already there
            Directory.CreateDirectory(path + "/plugins");

            // Place either the 64-bits or 32-bits DLL
            try
            {
                if (path.Contains("win_x64"))
                    File.WriteAllBytes(path + "/plugins/ets2-telemetry-server.dll", Resources.ets2_telemetry_server_x64);
                else
                    File.WriteAllBytes(path + "/plugins/ets2-telemetry-server.dll", Resources.ets2_telemetry_server_x86);

                MainManager.Logger.Debug("Installed ETS2 plugin in {0}", path);
            }
            catch (Exception e)
            {
                MainManager.Logger.Error(e, "Failed to install ETS2 plugin in {0}", path);
                throw;
            }
            
        }
    }
}