using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Artemis.DAL;
using Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing;
using Artemis.Utilities;
using Caliburn.Micro;
using MahApps.Metro;
using Newtonsoft.Json;
using Squirrel;

namespace Artemis.Settings
{
    public class GeneralSettings : IArtemisSettings
    {
        public GeneralSettings()
        {
            ThemeManager.AddAccent("CorsairYellow", new Uri("pack://application:,,,/Styles/Accents/CorsairYellow.xaml"));
            ApplyAutorun();
        }

        [DefaultValue("GeneralProfile")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastModule { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastKeyboard { get; set; }

        [DefaultValue("Qwerty")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Layout { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool EnablePointersUpdate { get; set; }

        [DefaultValue(51364)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int GamestatePort { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Autorun { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Suspended { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ShowOnStartup { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoUpdate { get; set; }

        [DefaultValue("Light")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Theme { get; set; }

        [DefaultValue(20)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ScreenCaptureFPS { get; set; }

        [DefaultValue("Info")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LogLevel { get; set; }

        public Version LastRanVersion { get; set; }

        public void Save()
        {
            SettingsProvider.Save(this);

            Logging.SetupLogging(LogLevel);
            ApplyAutorun();
            ApplyTheme();
            ApplyGamestatePort();
            ApplyScreenCaptureFPS();
        }

        public void Reset(bool save = false)
        {
            JsonConvert.PopulateObject("{}", this, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Reuse
            });

            if (save)
                SettingsProvider.Save(this);
        }

        private void ApplyGamestatePort()
        {
            // TODO: Restart Gamestate server with new port
        }

        public void ApplyAutorun()
        {
            using (var mgr = new UpdateManager(""))
            {
                try
                {
                    if (Autorun)
                        mgr.CreateShortcutsForExecutable("Artemis.exe", ShortcutLocation.Startup, false, "--autorun");
                    else
                        mgr.RemoveShortcutsForExecutable("Artemis.exe", ShortcutLocation.Startup);
                }
                catch (FileNotFoundException)
                {
                    // Ignored, only happens when running from VS
                }
                catch (DirectoryNotFoundException)
                {
                    // Ignored, only happens when running from VS
                }

            }
        }

        public void ApplyTheme()
        {
            Execute.OnUIThread(delegate
            {
                switch (Theme)
                {
                    case "Light":
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Teal"),
                            ThemeManager.GetAppTheme("BaseLight"));
                        break;
                    case "Dark":
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Teal"),
                            ThemeManager.GetAppTheme("BaseDark"));
                        break;
                    case "Corsair Light":
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("CorsairYellow"),
                            ThemeManager.GetAppTheme("BaseLight"));
                        break;
                    case "Corsair Dark":
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("CorsairYellow"),
                            ThemeManager.GetAppTheme("BaseDark"));
                        break;
                }
            });
        }

        public void ApplyScreenCaptureFPS()
        {
            ScreenCaptureManager.UpdateRate = 1.0 / ScreenCaptureFPS;
        }
    }
}