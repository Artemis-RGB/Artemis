using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;
using Squirrel;

namespace Artemis.Utilities
{
    public static class Updater
    {
        public static async void UpdateApp()
        {
            // Only update if the user allows it
            if (!General.Default.AutoUpdate)
                return;

            using (var mgr = new UpdateManager("http://artemis-rgb.com/auto-update"))
            {
                // Replace / remove the autorun shortcut
                SquirrelAwareApp.HandleEvents(onAppUpdate: v => AppUpdate(mgr), onAppUninstall: v => AppUninstall(mgr));
                
                await mgr.UpdateApp();
            }
        }

        private static void AppUpdate(IUpdateManager mgr)
        {
            var settings = new GeneralSettings();

            settings.ApplyAutorun();
            mgr.CreateShortcutForThisExe();
        }

        private static void AppUninstall(IUpdateManager mgr)
        {
            var settings = new GeneralSettings {Autorun = false};

            settings.ApplyAutorun();
            mgr.RemoveShortcutForThisExe();
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
                var json = jsonClient.DownloadString(
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