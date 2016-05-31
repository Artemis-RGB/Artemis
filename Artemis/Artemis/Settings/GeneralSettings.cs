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
        private readonly Accent _artemisAccent = ThemeManager.GetAccent("Teal");
        private readonly Accent _corsairAccent = new Accent("CorsairYellow",
            new Uri("pack://application:,,,/Styles/Accents/CorsairYellow.xaml"));
        private readonly AppTheme _darkTheme = ThemeManager.GetAppTheme("BaseDark");
        private readonly AppTheme _lightTheme = ThemeManager.GetAppTheme("BaseLight");

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

        public bool CheckForUpdates
        {
            get { return General.Default.CheckForUpdates; }
            set
            {
                if (General.Default.CheckForUpdates == value) return;
                General.Default.CheckForUpdates = value;
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

        private void ApplyGamestatePort()
        {
            // TODO: Restart Gamestate server with new port
        }

        private void ApplyAutorun()
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
        }

        // TODO: http://visionarycoder.com/2015/01/06/setting-themeaccent-with-mahapps-metro/
        private void ApplyTheme()
        {
            switch (Theme)
            {
                case "Light":
                    ThemeManager.ChangeAppStyle(Application.Current, _artemisAccent, _lightTheme);
                    break;
                case "Dark":
                    ThemeManager.ChangeAppStyle(Application.Current, _artemisAccent, _darkTheme);
                    break;
                case "Corsair Light":
                    ThemeManager.ChangeAppStyle(Application.Current, _corsairAccent, _lightTheme);
                    break;
                case "Corsair Dark":
                    ThemeManager.ChangeAppStyle(Application.Current, _corsairAccent, _darkTheme);
                    break;
            }
        }

        public void ResetSettings()
        {
            GamestatePort = 51364;
            EnablePointersUpdate = true;
            Autorun = true;
            CheckForUpdates = true;
            ShowOnStartup = true;
            Theme = "Light";

            SaveSettings();
        }
    }
}