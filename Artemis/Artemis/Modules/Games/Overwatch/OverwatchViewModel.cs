using System.IO;
using System.Windows.Forms;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Utilities.DataReaders;
using Artemis.ViewModels.Abstract;
using Microsoft.Win32;

namespace Artemis.Modules.Games.Overwatch
{
    public sealed class OverwatchViewModel : GameViewModel
    {
        public OverwatchViewModel(MainManager main, IProfileEditorVmFactory pFactory, OverwatchModel model)
            : base(main, model, pFactory)
        {
            DisplayName = "Overwatch";

            FindOverwatch();
        }

        public void FindOverwatch()
        {
            var gameSettings = (OverwatchSettings) GameSettings;
            // If already propertly set up, don't do anything
            if ((gameSettings.GameDirectory != null) && File.Exists(gameSettings.GameDirectory + "Overwatch.exe") &&
                File.Exists(gameSettings.GameDirectory + "RzChromaSDK64.dll"))
                return;

            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Overwatch");
            if (key == null)
                return;

            var path = key.GetValue("DisplayIcon").ToString();
            if (!File.Exists(path))
                return;

            gameSettings.GameDirectory = path.Substring(0, path.Length - 14);
            gameSettings.Save();
            PlaceDll();
        }

        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog {SelectedPath = ((OverwatchSettings) GameSettings).GameDirectory};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ((OverwatchSettings) GameSettings).GameDirectory = dialog.SelectedPath;
            NotifyOfPropertyChange(() => GameSettings);
            GameSettings.Save();

            PlaceDll();
        }

        public void PlaceDll()
        {
            var path = ((OverwatchSettings) GameSettings).GameDirectory;

            if (!File.Exists(path + @"\Overwatch.exe"))
            {
                DialogService.ShowErrorMessageBox("Please select a valid Overwatch directory\n\n" +
                                                  @"By default Overwatch is in C:\Program Files (x86)\Overwatch");

                ((OverwatchSettings) GameSettings).GameDirectory = string.Empty;
                NotifyOfPropertyChange(() => GameSettings);
                GameSettings.Save();
                return;
            }

            DllManager.PlaceRazerDll(path);
        }
    }
}