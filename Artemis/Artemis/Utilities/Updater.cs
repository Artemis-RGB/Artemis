using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using LogManager = NLog.LogManager;

namespace Artemis.Utilities
{
    public static class Updater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Checks to see if a new version is available on GitHub asks the user to see changes and download
        /// </summary>
        /// <param name="dialogService">The dialog service to use for progress and result dialogs</param>
        /// <returns></returns>
        public static async Task<bool?> CheckForUpdate(MetroDialogService dialogService)
        {
            // Check GitHub for a new version
            var jsonClient = new WebClient();

            // GitHub trips if we don't add a user agent
            jsonClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            // Random number to get around cache issues
            var rand = new Random(DateTime.Now.Millisecond);
            string json;
            try
            {
                json = await jsonClient.DownloadStringTaskAsync("https://api.github.com/repos/SpoinkyNL/Artemis/releases/latest?random=" + rand.Next());
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Update check failed.");
                return null;
            }

            var release = JObject.Parse(json);
            var releaseVersion = Version.Parse(release["tag_name"].Value<string>());
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (releaseVersion > currentVersion)
            {
                await Execute.OnUIThreadAsync(async () => await ShowChanges(dialogService, release));
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Shows the changes for the given release and offers to download it
        /// </summary>
        /// <param name="dialogService">The dialog service to use for progress and result dialogs</param>
        /// <param name="release">The release to show and offer the download for</param>
        /// <returns></returns>
        private static async Task ShowChanges(MetroDialogService dialogService, JObject release)
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Download & install",
                NegativeButtonText = "Ask again later"
            };

            var update = await dialogService.ShowMarkdownDialog(release["name"].Value<string>(), release["body"].Value<string>(), settings);
            if (update == null || (bool) !update)
                return;

            // Show a process dialog 
            var dialog = await dialogService.ShowProgressDialog("Applying update", "The new update is being downloaded right now...");
            dialog.SetIndeterminate();
            // Download the release file, it's the one starting with "artemis-setup"
            var releaseFile = release["assets"].Children().FirstOrDefault(c => c["name"].Value<string>().StartsWith("artemis-setup") &&
                                                                               c["name"].Value<string>().EndsWith(".exe"));
            // If there's no matching release it means whoever published the new version fucked up, can't do much about that
            if (releaseFile == null)
            {
                await dialog.CloseAsync();
                dialogService.ShowMessageBox("Applying update failed", "Couldn't find the update file. Please install the latest version manually, sorry!");
                return;
            }

            var downloadClient = new WebClient();
            downloadClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Task<byte[]> download;
            try
            {
                download = downloadClient.DownloadDataTaskAsync(releaseFile["browser_download_url"].Value<string>());
            }
            catch (Exception e)
            {
                dialogService.ShowMessageBox("Applying update failed", "We ran into an issue downloaidng the update: \n\n" + e.Message);
                Logger.Warn(e, "Update check failed.");
                return;
            }
            downloadClient.DownloadProgressChanged += (sender, args) =>
            {
                dialog.SetMessage("The new update is being downloaded right now...\n\n" +
                                  $"Progress: {ConvertBytesToMegabytes(args.BytesReceived)} MB/{ConvertBytesToMegabytes(args.TotalBytesToReceive)} MB");
                dialog.SetProgress(args.ProgressPercentage / 100.0);
            };
            var setupBytes = await download;
            dialog.SetMessage("Installing the new update...");
            dialog.SetIndeterminate();

            // Ensure the update folder exists
            var artemisFolder = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
            var updateFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Artemis\\updates";
            var updatePath = updateFolder + "\\" + releaseFile["name"].Value<string>();
            if (!Directory.Exists(updateFolder))
                Directory.CreateDirectory(updateFolder);

            // Store the bytes
            File.WriteAllBytes(updatePath, setupBytes);
            // Create a bat file that'll take care of the installation (Artemis gets shut down during install) the bat file will
            // carry forth our legacy (read that in an heroic tone)
            var updateScript = "ECHO OFF\r\n" +
                               "CLS\r\n" +
                               $"\"{updatePath}\" /passive\r\n" +
                               $"cd \"{artemisFolder}\"\r\n" +
                               "start Artemis.exe --show";
            File.WriteAllText(updateFolder + "\\updateScript.bat", updateScript);
            var psi = new ProcessStartInfo
            {
                FileName = updateFolder + "\\updateScript.bat",
                Verb = "runas"
            };

            var process = new System.Diagnostics.Process {StartInfo = psi};
            process.Start();
            process.WaitForExit();
        }

        private static object ConvertBytesToMegabytes(long bytes)
        {
            return Math.Round(bytes / 1024f / 1024f, 2);
        }

        /// <summary>
        ///     Queries GitHub for the latest pointers file
        /// </summary>
        public static void GetPointers()
        {
            if (!SettingsProvider.Load<GeneralSettings>().EnablePointersUpdate)
            {
                LoadNullDefaults();
                return;
            }

            try
            {
                var jsonClient = new WebClient();
                var offsetSettings = SettingsProvider.Load<OffsetSettings>();
                // Random number to get around cache issues
                var rand = new Random(DateTime.Now.Millisecond);
                var json = jsonClient.DownloadString("https://raw.githubusercontent.com/SpoinkyNL/" +
                                                     "Artemis/master/pointers.json?random=" + rand.Next());

                // Get a list of pointers
                var pointers = JsonConvert.DeserializeObject<List<GamePointersCollection>>(json);

                // Assign each pointer to the settings file
                if (pointers.FirstOrDefault(p => p.Game == "RocketLeague") != null)
                    offsetSettings.RocketLeague = pointers.FirstOrDefault(p => p.Game == "RocketLeague");
                if (pointers.FirstOrDefault(p => p.Game == "WorldOfWarcraft") != null)
                    offsetSettings.WorldOfWarcraft = pointers.FirstOrDefault(p => p.Game == "WorldOfWarcraft");
                if (pointers.FirstOrDefault(p => p.Game == "Terraria") != null)
                    offsetSettings.Terraria = pointers.FirstOrDefault(p => p.Game == "Terraria");

                offsetSettings.Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Removes the old Squirrel-based version of Artemis if present
        /// </summary>
        public static void CleanSquirrel()
        {
            var squirrelPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Artemis";
            if (!Directory.Exists(squirrelPath))
                return;

            var psi = new ProcessStartInfo
            {
                FileName = squirrelPath + "\\Update.exe",
                Arguments = "--uninstall",
                Verb = "runas"
            };

            var process = new System.Diagnostics.Process {StartInfo = psi};
            process.Start();
            process.WaitForExit();

            Directory.Delete(squirrelPath, true);
        }

        /// <summary>
        ///     JSON default value handling can only go so far, so the update will take care of defaults
        ///     on the offsets if they are null
        /// </summary>
        private static void LoadNullDefaults()
        {
            var offsetSettings = SettingsProvider.Load<OffsetSettings>();
            if (offsetSettings.RocketLeague == null)
            {
                offsetSettings.RocketLeague = new GamePointersCollection
                {
                    Game = "RocketLeague",
                    GameVersion = "1.21",
                    GameAddresses = new List<GamePointer>
                    {
                        new GamePointer
                        {
                            Description = "Boost",
                            BasePointer = new IntPtr(0x016AD528),
                            Offsets = new[] {0x304, 0x8, 0x50, 0x720, 0x224}
                        }
                    }
                };
            }

            offsetSettings.Save();
        }
    }
}
