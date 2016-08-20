using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artemis.Settings;
using Newtonsoft.Json;
using NLog;

namespace Artemis.DAL
{
    public static class SettingsProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string SettingsFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\settings";

        private static readonly List<IArtemisSettings> Settings = new List<IArtemisSettings>();

        /// <summary>
        ///     Loads settings with the given name from the filesystem
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Load<T>(string name) where T : new()
        {
            if (!AreSettings(typeof(T)))
                throw new ArgumentException("Type doesn't implement IArtemisSettings");

            // Attempt to load from memory first
            var inMemory = Settings.FirstOrDefault(s => s.Name == name);
            if (inMemory != null)
                return (T) inMemory;

            CheckSettings();

            try
            {
                var loadSettings = (IArtemisSettings) JsonConvert
                    .DeserializeObject<T>(File.ReadAllText(SettingsFolder + $@"\{name}.json"));
                if (loadSettings == null)
                    SetToDefault(ref loadSettings);

                Settings.Add(loadSettings);
                return (T) loadSettings;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't load settings '{0}.json'", name);

                // Not sure about this, I've seen prettier code
                var loadSettings = (IArtemisSettings) new T();
                SetToDefault(ref loadSettings);
                Settings.Add(loadSettings);
                return (T) loadSettings;
            }
        }

        /// <summary>
        ///     Saves the settings object to the filesystem
        /// </summary>
        /// <param name="artemisSettings"></param>
        public static void Save(IArtemisSettings artemisSettings)
        {
            CheckSettings();

            string json;
            // Should saving fail for whatever reason, catch the exception and log it
            // But DON'T touch the settings file.
            try
            {
                json = JsonConvert.SerializeObject(artemisSettings, Formatting.Indented);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't save settings '{0}.json'", artemisSettings.Name);
                return;
            }

            File.WriteAllText(SettingsFolder + $@"\{artemisSettings.Name}.json", json);
        }

        /// <summary>
        ///     Restores the settings object to its default values
        /// </summary>
        /// <param name="settings"></param>
        public static void SetToDefault(ref IArtemisSettings settings)
        {
            // Loading the object from an empty JSON object makes Json.NET use all the default values
            settings = (IArtemisSettings) JsonConvert.DeserializeObject("{}", settings.GetType());
        }

        private static void CheckSettings()
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);
        }

        private static bool AreSettings(Type t)
        {
            return typeof(IArtemisSettings).IsAssignableFrom(t);
        }
    }
}