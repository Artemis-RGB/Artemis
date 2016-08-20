using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;
using Squirrel;
using SettingsProvider = Artemis.DAL.SettingsProvider;

namespace Artemis.Utilities
{
    public static class Updater
    {
        public static async void UpdateApp()
        {
            // Only update if the user allows it
            if (!SettingsProvider.Load<GeneralSettings>("GeneralSettings").AutoUpdate)
                return;

            // TODO: Remove prerelease before releasing
            //            using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/SpoinkyNL/Artemis", null, null, null,true))
            //            {
            //                // Replace / remove the autorun shortcut
            //                SquirrelAwareApp.HandleEvents(onAppUpdate: v => AppUpdate(mgr.Result),
            //                    onAppUninstall: v => AppUninstall(mgr.Result));
            //
            //                await mgr.Result.UpdateApp();
            //            }

            using (var mgr = new UpdateManager("C:\\Users\\Robert\\Desktop\\Artemis builds\\squirrel_test"))
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
            // Use GeneralSettings to get rid of the autorun shortcut
            var fakeSettings = new GeneralSettings { Autorun = false };
            fakeSettings.ApplyAutorun();

            mgr.RemoveShortcutForThisExe();
        }

        public static void GetPointers()
        {
            if (!DAL.SettingsProvider.Load<GeneralSettings>("GeneralSettings").EnablePointersUpdate)
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