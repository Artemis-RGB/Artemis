using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.Witcher3
{
    public sealed class Witcher3ViewModel : GameViewModel
    {
        public Witcher3ViewModel(MainManager main, IProfileEditorVmFactory pFactory, Witcher3Model model)
            : base(main, model, pFactory)
        {
            DisplayName = "The Witcher 3";
        }

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
                    DialogService.ShowQuestionMessageBox("Installation error",
                        "That's not a valid Witcher 3 directory\n\n" +
                        "Default directories:\n" +
                        "Steam: \\SteamApps\\common\\The Witcher 3\n" +
                        "GOG: C:\\GOG Games\\The Witcher 3 Wild Hunt\n\n" +
                        "Retry?");
                if (retry.Value)
                    AutoInstall();
                return;
            }

            // Load the ZIP from resources
            var stream = new MemoryStream(Resources.witcher3_mod);
            var archive = new ZipArchive(stream);

            // Look for any conflicting mods
            if (Directory.Exists(dialog.SelectedPath + @"\mods"))
            {
                var file =
                    Directory.GetFiles(dialog.SelectedPath + @"\mods", "playerWitcher.ws", SearchOption.AllDirectories)
                        .FirstOrDefault();
                if (file != null)
                    if (!file.Contains("modArtemis"))
                    {
                        var viewHelp = await
                            DialogService.ShowQuestionMessageBox("Conflicting mod found",
                                "Oh no, you have a conflicting mod!\n\n" +
                                $"Conflicting file: {file.Remove(0, dialog.SelectedPath.Length)}\n\n" +
                                "Would you like to view instructions on how to manually install the mod?");
                        if (!viewHelp.Value)
                            return;

                        // Put the mod in the documents folder instead
                        // Create the directory structure
                        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis";

                        archive.ExtractToDirectory(folder + @"witcher3-mod", true);

                        Process.Start(
                            new ProcessStartInfo("https://github.com/SpoinkyNL/Artemis/wiki/The-Witcher-3"));
                        return;
                    }
            }

            archive.ExtractToDirectory(dialog.SelectedPath, true);
            DialogService.ShowMessageBox("Success", "The mod was successfully installed!");
        }
    }
}