using System;
using System.ComponentModel;
using System.IO;
using System.Web.Security;
using System.Windows;
using Artemis.DAL;
using Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.Utilities.ActiveWindowDetection;
using Caliburn.Micro;
using MahApps.Metro;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        [DefaultValue(ActiveWindowDetectionType.Events)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ActiveWindowDetectionType ActiveWindowDetection { get; set; }

        public Version LastRanVersion { get; set; }

        public void Save()
        {
            SettingsProvider.Save(this);

            Logging.SetupLogging(LogLevel);
            ActiveWindowHelper.SetActiveWindowDetectionType(ActiveWindowDetection);
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
            using (var ts = new TaskService())
            {
                var existing = ts.FindTask("Artemis autorun");
                if (Autorun)
                {
                    // Overwrite any existing tasks in case the installation folder changed
                    var path = Path.GetTempFileName();
                    var artemisPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
                    var xml = Resources.Artemis_autorun
                        .Replace("{{artemisPath}}", AppDomain.CurrentDomain.BaseDirectory)
                        .Replace("{{author}}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                    File.WriteAllText(path, xml);

                    var task = ts.RootFolder.ImportTask(null, path);
                    task.Definition.Principal.UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    task.Definition.Principal.LogonType = TaskLogonType.InteractiveToken;
                    task.Definition.Principal.RunLevel = TaskRunLevel.Highest;
                    task.RegisterChanges();

                    File.Delete(path);
                }
                else if (existing != null)
                {
                    // Remove the task if it is present
                    ts.RootFolder.DeleteTask("Artemis autorun");
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
