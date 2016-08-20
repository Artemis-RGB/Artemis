using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using Artemis.DAL;
using Artemis.Utilities;
using MahApps.Metro;
using Newtonsoft.Json;

namespace Artemis.Settings
{
    public class GeneralSettings : IArtemisSettings
    {
        public GeneralSettings()
        {
            ThemeManager.AddAccent("CorsairYellow", new Uri("pack://application:,,,/Styles/Accents/CorsairYellow.xaml"));
            ApplyTheme();
        }

        [DefaultValue("WindowsProfile")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastEffect { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastKeyboard { get; set; }

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

        [DefaultValue("Info")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LogLevel { get; set; }

        public string Name { get; } = "GeneralSettings";

        public void Save()
        {
            SettingsProvider.Save(this);
            ApplyAutorun();
            ApplyTheme();
            ApplyGamestatePort();
            Logging.SetupLogging(LogLevel);
        }

        private void ApplyGamestatePort()
        {
            // TODO: Restart Gamestate server with new port
        }

        public void ApplyAutorun()
        {
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Autorun)
            {
                var link = (IShellLink) new ShellLink();
                link.SetPath(Assembly.GetExecutingAssembly().Location);
                var file = (IPersistFile) link;

                file.Save(startupFolder + @"\Artemis.lnk", false);
            }
            else if (File.Exists(startupFolder + @"\Artemis.lnk"))
                File.Delete(startupFolder + @"\Artemis.lnk");
        }

        private void ApplyTheme()
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
        }
    }
}