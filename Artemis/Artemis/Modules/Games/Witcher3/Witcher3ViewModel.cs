using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3ViewModel : GameViewModel
    {
        public Witcher3ViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            GameSettings = new Witcher3Settings();

            // Create effect model and add it to MainManager
            GameModel = new Witcher3Model(mainManager, (Witcher3Settings) GameSettings);
            MainManager.EffectManager.EffectModels.Add(GameModel);
        }

        public static string Name => "The Witcher 3";

        public async void AutoInstall()
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
                var retry = await
                    MainManager.DialogService.ShowQuestionMessageBox("Installation error",
                        "That's not a valid Witcher 3 directory\n\n" +
                        "Default directories:\n" +
                        "Steam: \\SteamApps\\common\\The Witcher 3\n" +
                        "GOG: C:\\GOG Games\\The Witcher 3 Wild Hunt\n\n" +
                        "Retry?");
                if (retry.Value)
                    AutoInstall();
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
                        var viewHelp = await
                            MainManager.DialogService.ShowQuestionMessageBox("Conflicting mod found",
                                "Oh no, you have a conflicting mod!\n\n" +
                                "Conflicting file: " + file.Remove(0, dialog.SelectedPath.Length) +
                                "\n\nWould you like to view instructions on how to manually install the mod?");
                        if (!viewHelp.Value)
                            return;

                        // Put the mod in the documents folder instead
                        // Create the directory structure
                        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis";
                        if (
                            !Directory.Exists(folder + @"\Witcher3\mods\modArtemis\content\scripts\game\player"))
                            Directory.CreateDirectory(folder +
                                                      @"\Witcher3\mods\modArtemis\content\scripts\game\player");
                        if (
                            !Directory.Exists(folder + @"\Witcher3\bin\config\r4game\user_config_matrix\pc"))
                            Directory.CreateDirectory(folder + @"\Witcher3\bin\config\r4game\user_config_matrix\pc");

                        // Install the mod files
                        File.WriteAllText(folder + @"\Witcher3\bin\config\r4game\user_config_matrix\pc\artemis.xml",
                            Resources.artemisXml);
                        File.WriteAllText(
                            folder + @"\Witcher3\mods\modArtemis\content\scripts\game\player\playerWitcher.ws",
                            Resources.playerWitcherWs);

                        Process.Start(new ProcessStartInfo("https://github.com/SpoinkyNL/Artemis/wiki/The-Witcher-3"));
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

            MainManager.DialogService.ShowMessageBox("Success", "The mod was successfully installed!");
        }
    }
}