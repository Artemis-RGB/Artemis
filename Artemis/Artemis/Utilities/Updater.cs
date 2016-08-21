using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Artemis.DAL;
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
            if (!SettingsProvider.Load<GeneralSettings>().AutoUpdate)
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
            var fakeSettings = new GeneralSettings {Autorun = false};
            fakeSettings.ApplyAutorun();

            mgr.RemoveShortcutForThisExe();
        }

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