using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Artemis.Utilities
{
    public static class Updater
    {
        public static int CurrentVersion = 1211;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<Action> CheckForUpdate(MetroDialogService dialogService)
        {
            Logger.Info("Checking for updates - Current version: 1.2.1.1 beta");
            if (!General.Default.CheckForUpdates)
                return null;

            var newRelease = IsUpdateAvailable();
            if (newRelease == null)
            {
                Logger.Info("No update found.");
                return null;
            }

            Logger.Info("Found new version - {0}.", newRelease["tag_name"].Value<string>());
            var viewUpdate = await
                dialogService.ShowQuestionMessageBox("ApplyProperties available",
                    $"A new version of Artemis is available, version {newRelease["tag_name"].Value<string>()}.\n" +
                    "Do you wish to view the update on GitHub now?\n\n" +
                    "Note: You can disable update notifications in the settings menu");

            if (viewUpdate.Value)
                System.Diagnostics.Process.Start(new ProcessStartInfo(newRelease["html_url"].Value<string>()));

            return null;
        }

        public static JObject IsUpdateAvailable()
        {
            if (!General.Default.EnablePointersUpdate)
                return null;

            try
            {
                var jsonClient = new WebClient();

                // GitHub trips if we don't add a user agent
                jsonClient.Headers.Add("user-agent",
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                // Random number to get around cache issues
                var rand = new Random(DateTime.Now.Millisecond);
                var json =
                    jsonClient.DownloadString("https://api.github.com/repos/SpoinkyNL/Artemis/releases/latest?random=" +
                                              rand.Next());

                // Get a list of pointers
                var release = JsonConvert.DeserializeObject<JObject>(json);

                // Parse a version number string to an int
                var remoteVersion = int.Parse(release["tag_name"].Value<string>().Replace(".", ""));

                return remoteVersion > CurrentVersion ? release : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void GetPointers()
        {
            if (!General.Default.EnablePointersUpdate)
                return;

            try
            {
                var jsonClient = new WebClient();

                // Random number to get around cache issues
                var rand = new Random(DateTime.Now.Millisecond);
                var json =
                    jsonClient.DownloadString(
                        "https://raw.githubusercontent.com/SpoinkyNL/Artemis/master/pointers.json?random=" + rand.Next());

                // Get a list of pointers
                var pointers = JsonConvert.DeserializeObject<List<GamePointersCollection>>(json);
                // Assign each pointer to the settings file
                var rlPointers = JsonConvert.SerializeObject(pointers.FirstOrDefault(p => p.Game == "RocketLeague"));
                if (rlPointers != null)
                {
                    Offsets.Default.RocketLeague = rlPointers;
                    Offsets.Default.Save();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}