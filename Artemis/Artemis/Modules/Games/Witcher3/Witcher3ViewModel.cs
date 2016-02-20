using System.IO;
using System.Linq;
using System.Windows.Forms;
using Artemis.Models;
using Artemis.Properties;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3ViewModel : Screen
    {
        private Witcher3Settings _witcher3Settings;

        public Witcher3ViewModel(MainModel mainModel)
        {
            MainModel = mainModel;

            // Settings are loaded from file by class
            Witcher3Settings = new Witcher3Settings();

            // Create effect model and add it to MainModel
            Witcher3Model = new Witcher3Model(mainModel, Witcher3Settings);
            MainModel.EffectModels.Add(Witcher3Model);
        }

        public static string Name => "The Witcher 3";

        public MainModel MainModel { get; set; }
        public Witcher3Model Witcher3Model { get; set; }

        public Witcher3Settings Witcher3Settings
        {
            get { return _witcher3Settings; }
            set
            {
                if (Equals(value, _witcher3Settings)) return;
                _witcher3Settings = value;
                NotifyOfPropertyChange(() => Witcher3Settings);
            }
        }

        public void SaveSettings()
        {
            if (Witcher3Model == null)
                return;

            Witcher3Settings.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            Witcher3Settings.ToDefault();
            NotifyOfPropertyChange(() => Witcher3Settings);

            SaveSettings();
        }

        public void ToggleEffect()
        {
            Witcher3Model.Enabled = _witcher3Settings.Enabled;
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
                                        "\n\nOnce you press OK you will be taken to an instructions page.",
                            "Conflicting mod found");
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
            File.WriteAllText(dialog.SelectedPath + @"\bin\config\r4game\user_config_matrix\pc\artemis.xml",
                Resources.artemisXml);
            File.WriteAllText(dialog.SelectedPath + @"\mods\modArtemis\content\scripts\game\player\playerWitcher.ws",
                Resources.playerWitcherWs);

            MessageBox.Show("The mod was successfully installed!", "Success");
        }
    }
}