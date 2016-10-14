using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Squirrel;

namespace Artemis.Utilities
{
    public static class Updater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Uses Squirrel to update the application through GitHub
        /// </summary>
        public static async void UpdateApp()
        {
            var settings = SettingsProvider.Load<GeneralSettings>();
            Logger.Info("Update check enabled: {0}", settings.AutoUpdate);

            // Only update if the user allows it
            if (!SettingsProvider.Load<GeneralSettings>().AutoUpdate)
                return;
            
            // Pre-release
            // using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/SpoinkyNL/Artemis", null, null, null, true))
            // Release
            using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/SpoinkyNL/Artemis"))
            {
                try
                {
                    await mgr.Result.UpdateApp();
                    Logger.Info("Update check complete");
                    mgr.Result.Dispose(); // This seems odd but if it's not disposed and exception is thrown
                }
                catch (Exception e)
                {
                    // These exceptions should only really occur when running from VS
                    Logger.Error(e, "Update check failed");
                    mgr.Result.Dispose();
                }
            }
        }

        /// <summary>
        ///     Checks to see if the program has updated and shows a dialog if so.
        /// </summary>
        /// <param name="dialogService">The dialog service to use for progress and result dialogs</param>
        /// <returns></returns>
        public static async Task CheckChangelog(MetroDialogService dialogService)
        {
            var settings = SettingsProvider.Load<GeneralSettings>();
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if ((settings.LastRanVersion != null) && (currentVersion > settings.LastRanVersion))
            {
                Logger.Info("Updated from {0} to {1}, showing changelog.", settings.LastRanVersion, currentVersion);

                // Ask the user whether he/she wants to see what's new
                var showChanges = await dialogService.
                    ShowQuestionMessageBox("New version installed",
                        $"Artemis has recently updated from version {settings.LastRanVersion} to {currentVersion}. \n" +
                        "Would you like to see what's new?");

                // If user wants to see changelog, show it to them
                if ((showChanges != null) && showChanges.Value)
                    await ShowChanges(dialogService, currentVersion);
            }

            settings.LastRanVersion = currentVersion;
            settings.Save();
        }

        /// <summary>
        ///     Fetches all releases from GitHub, looks up the current release and shows the changelog
        /// </summary>
        /// <param name="dialogService">The dialog service to use for progress and result dialogs</param>
        /// <param name="version">The version to fetch the changelog for</param>
        /// <returns></returns>
        private static async Task ShowChanges(MetroDialogService dialogService, Version version)
        {
            var progressDialog = await dialogService.ShowProgressDialog("Changelog", "Fetching release data from GitHub..");
            progressDialog.SetIndeterminate();

            var jsonClient = new WebClient();

            // GitHub trips if we don't add a user agent
            jsonClient.Headers.Add("user-agent",
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            // Random number to get around cache issues
            var rand = new Random(DateTime.Now.Millisecond);
            var json = await jsonClient.DownloadStringTaskAsync(
                "https://api.github.com/repos/SpoinkyNL/Artemis/releases?random=" + rand.Next());

            // Get a list of releases
            var releases = JsonConvert.DeserializeObject<JArray>(json);
            var release = releases.FirstOrDefault(r => r["tag_name"].Value<string>() == version.ToString());
            try
            {
                await progressDialog.CloseAsync();
            }
            catch (InvalidOperationException)
            {
                // Occurs when main window is closed before finished
            }

            if (release != null)
                dialogService.ShowMessageBox(release["name"].Value<string>(), release["body"].Value<string>());
            else
                dialogService.ShowMessageBox("Couldn't fetch release",
                    "Sorry, Artemis was unable to fetch the release data off of GitHub.\n" +
                    "If you'd like, you can always find out the latest changes on the GitHub page accessible from the options menu");
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

                offsetSettings.Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     JSON default value handling can only go so far, so the update will take care of defaults
        ///     on the offsets if they are null
        /// </summary>
        private static void LoadNullDefaults()
        {
            var offsetSettings = SettingsProvider.Load<OffsetSettings>();
            if (offsetSettings.RocketLeague == null)
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

            offsetSettings.Save();
        }
    }
}