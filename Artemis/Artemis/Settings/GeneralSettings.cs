using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using Artemis.Utilities;
using MahApps.Metro;

namespace Artemis.Settings
{
    public class GeneralSettings
    {
        public GeneralSettings()
        {
            ThemeManager.AddAccent("CorsairYellow", new Uri("pack://application:,,,/Styles/Accents/CorsairYellow.xaml"));
            ApplyTheme();
        }

        public int GamestatePort
        {
            get { return General.Default.GamestatePort; }
            set
            {
                if (General.Default.GamestatePort == value) return;
                General.Default.GamestatePort = value;
            }
        }

        public bool EnablePointersUpdate
        {
            get { return General.Default.EnablePointersUpdate; }
            set
            {
                if (General.Default.EnablePointersUpdate == value) return;
                General.Default.EnablePointersUpdate = value;
            }
        }

        public bool Autorun
        {
            get { return General.Default.Autorun; }
            set
            {
                if (General.Default.Autorun == value) return;
                General.Default.Autorun = value;
            }
        }

        public bool AutoUpdate
        {
            get { return General.Default.AutoUpdate; }
            set
            {
                if (General.Default.AutoUpdate == value) return;
                General.Default.AutoUpdate = value;
            }
        }

        public bool ShowOnStartup
        {
            get { return General.Default.ShowOnStartup; }
            set
            {
                if (General.Default.ShowOnStartup == value) return;
                General.Default.ShowOnStartup = value;
            }
        }

        public string Theme
        {
            get { return General.Default.Theme; }
            set
            {
                if (General.Default.Theme == value) return;
                General.Default.Theme = value;
            }
        }

        public string LogLevel
        {
            get { return General.Default.LogLevel; }
            set
            {
                if (General.Default.LogLevel == value) return;
                General.Default.LogLevel = value;
            }
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

        public void SaveSettings()
        {
            General.Default.Save();

            ApplyAutorun();
            ApplyTheme();
            ApplyGamestatePort();
            Logging.SetupLogging(LogLevel);
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

        public void ResetSettings()
        {
            GamestatePort = 51364;
            EnablePointersUpdate = true;
            Autorun = true;
            AutoUpdate = true;
            ShowOnStartup = true;
            Theme = "Light";
            LogLevel = "Info";

            SaveSettings();
        }
    }
}