using System.IO;
using System.Linq;
using System.Windows.Forms;
using Artemis.Models;
using Artemis.Modules.Games.RocketLeague;
using Screen = Caliburn.Micro.Screen;

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

        public void AutoInstall()
        {
            // Request The Witcher 3 folder
            var dialog = new FolderBrowserDialog
            {
                Description = "Please select your Witcher 3 install path (root directory)."
            };
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            // If the subfolder doesn't contain witcher3.exe, it's the wrong folder.
            if (!File.Exists(dialog.SelectedPath + @"\bin\x64\witcher3.exe"))
            {
                var error = MessageBox.Show("That's not a valid Witcher 3 directory\n\n" +
                                            "Default directories:\n" +
                                            "Steam: C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Witcher 3\n" +
                                            "GOG: C:\\GOG Games\\The Witcher 3 Wild Hunt", "Installation error",
                    MessageBoxButtons.RetryCancel);
                if (error == DialogResult.Retry)
                    AutoInstall();
                else
                    return;
            }

            // Look for any conflicting mods
            if (Directory.Exists(dialog.SelectedPath + @"\mods"))
            {
                var file =
                    Directory.GetFiles(dialog.SelectedPath + @"\mods", "playerWitcher.ws", SearchOption.AllDirectories)
                        .FirstOrDefault();
                if (file != null)
                {
                    // Don't trip over our own mod
                    if (!file.Contains("modArtemis"))
                    {
                        MessageBox.Show("Oh no, you have a conflicting mod!\n\n" +
                            "Conflicting file: " + file.Remove(0, dialog.SelectedPath.Length) +
                            "\n\nOnce you press OK you will be taken to an instructions page.", "Conflicting mod found");
                        return;
                    }
                }
            }

            // Create the directory structure
            if (!Directory.Exists(dialog.SelectedPath + @"\mods\modArtemis\content\scripts\game\player"))
                Directory.CreateDirectory(dialog.SelectedPath + @"\mods\modArtemis\content\scripts\game\player");
            if (!Directory.Exists(dialog.SelectedPath + @"\bin\config\r4game\user_config_matrix\pc"))
                Directory.CreateDirectory(dialog.SelectedPath + @"\bin\config\r4game\user_config_matrix\pc");

            // Install the mod files
            File.WriteAllText(dialog.SelectedPath + @"\bin\config\r4game\user_config_matrix\pc\artemis.xml", Properties.Resources.artemis);
            File.WriteAllText(dialog.SelectedPath + @"\mods\modArtemis\content\scripts\game\player\playerWitcher.ws", Properties.Resources.playerWitcher);

            MessageBox.Show("The mod was successfully installed!", "Success");
        }
    }
}